// -----------------------------------------------------------------------
// <copyright file="ServerUpdater.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Class to manage the update process of a Godot game server.
/// </summary>
public class ServerUpdater
{
    private readonly string repoOwner;
    private readonly string repoName;
    private readonly string serverInstallPath;
    private readonly string backupPath;
    private readonly string currentVersionFile;
    private readonly HttpClient httpClient;
    private readonly string? githubToken; // Optional, needed for private repos
    private readonly Action<string> updateMessageAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerUpdater"/> class.
    /// </summary>
    /// <param name="repoOwner">The owner of the GitHub repository. Example: rob011235.</param>
    /// <param name="backupPath">Path to store backup files.</param>
    /// <param name="repoName">The name of the GitHub repository. Example: PointOfNoReturnGameServer.</param>
    /// <param name="updateMessageAction">Action to handle update messages.</param>
    /// <param name="serverInstallPath">The path where the server is installed.</param>
    /// <param name="githubToken">GitHub token for private repositories (optional).</param>
    public ServerUpdater(
        string repoOwner,
        string repoName,
        string serverInstallPath,
        Action<string>? updateMessageAction = null,
        string? backupPath = null,
        string? githubToken = null)
    {
        this.repoOwner = repoOwner;
        this.repoName = repoName;
        this.serverInstallPath = serverInstallPath;
        this.backupPath = backupPath ?? Path.Combine(Path.GetTempPath(), "godot_server_backup");
        this.currentVersionFile = Path.Combine(this.serverInstallPath, "version.txt");
        this.githubToken = githubToken;
        this.updateMessageAction = updateMessageAction ?? Console.WriteLine;

        this.httpClient = new HttpClient();
        this.httpClient.DefaultRequestHeaders.Add("User-Agent", "GodotServerUpdater");

        if (!string.IsNullOrEmpty(this.githubToken))
        {
            this.httpClient.DefaultRequestHeaders.Add("Authorization", $"token {this.githubToken}");
        }
    }

    /// <summary>
    /// Checks for updates and applies them if available.
    /// </summary>
    /// <returns>True if an update was applied, false otherwise.</returns>
    public async Task<bool> CheckAndUpdate()
    {
        try
        {
            this.updateMessageAction("Checking for updates...");

            // Get current version
            string currentVersion = "0.0.0";
            if (File.Exists(this.currentVersionFile))
            {
                currentVersion = File.ReadAllText(this.currentVersionFile).Trim();
            }

            this.updateMessageAction($"Current version: {currentVersion}");

            // Get latest release from GitHub
            var latestRelease = await this.GetLatestRelease();
            if (latestRelease == null)
            {
                this.updateMessageAction("Could not retrieve latest release information.");
                return false;
            }

            string? latestVersion = latestRelease.TagName;
            this.updateMessageAction($"Latest version: {latestVersion}");

            // Check if update is needed
            if (latestVersion == currentVersion)
            {
                this.updateMessageAction("Server is up to date.");
                return false;
            }

            this.updateMessageAction("Update available! Starting update process...");

            // Download the latest release
            string? assetUrl = latestRelease?.Assets?.Count > 0
                ? latestRelease.Assets[0].BrowserDownloadUrl
                : latestRelease?.ZipballUrl;
            if (string.IsNullOrEmpty(assetUrl))
            {
                this.updateMessageAction("No downloadable asset found for the latest release.");
                return false;
            }
            string downloadPath = await this.DownloadRelease(assetUrl);

            // Backup current server files
            this.BackupCurrentServer();

            // Stop the server
            this.StopServer();

            // Extract new files and install
            this.InstallUpdate(downloadPath);

            // Update version file
            File.WriteAllText(this.currentVersionFile, latestVersion);

            // Start the server again
            this.StartServer();

            this.updateMessageAction($"Successfully updated to version {latestVersion}");
            return true;
        }
        catch (Exception ex)
        {
            this.updateMessageAction($"Error during update: {ex.Message}");

            // Try to restore from backup if available
            if (Directory.Exists(this.backupPath))
            {
                this.updateMessageAction("Attempting to restore from backup...");
                try
                {
                    this.RestoreFromBackup();
                    this.StartServer();
                    this.updateMessageAction("Restored from backup.");
                }
                catch (Exception restoreEx)
                {
                    this.updateMessageAction($"Failed to restore from backup: {restoreEx.Message}");
                }
            }
            return false;
        }
    }

    private async Task<GitHubRelease?> GetLatestRelease()
    {
        string url = $"https://api.github.com/repos/{this.repoOwner}/{this.repoName}/releases/latest";
        HttpResponseMessage response = await this.httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GitHubRelease>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
        }

        return null;
    }

    private async Task<string> DownloadRelease(string assetUrl)
    {
        string downloadPath = Path.Combine(Path.GetTempPath(), $"godot_server_update_{Guid.NewGuid()}.zip");

        HttpResponseMessage response = await this.httpClient.GetAsync(assetUrl);
        response.EnsureSuccessStatusCode();

        using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await response.Content.CopyToAsync(fileStream);
        }

        return downloadPath;
    }

    private void BackupCurrentServer()
    {
        if (Directory.Exists(this.backupPath))
        {
            Directory.Delete(this.backupPath, true);
        }

        Directory.CreateDirectory(this.backupPath);

        foreach (string file in Directory.GetFiles(this.serverInstallPath, "*", SearchOption.AllDirectories))
        {
            string relativePath = file[(this.serverInstallPath.Length + 1) ..];
            string backupFilePath = Path.Combine(this.backupPath, relativePath);
            var directoryName = Path.GetDirectoryName(backupFilePath);
            if (directoryName != null && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            File.Copy(file, backupFilePath, true);
        }

        this.updateMessageAction("Backup created successfully.");
    }

    private void StopServer()
    {
        // This assumes the server is running as a process named "godot_server"
        // Adjust as needed for your specific setup
        Process[] processes = Process.GetProcessesByName("godot_server");
        foreach (var process in processes)
        {
            try
            {
                process.Kill();
                process.WaitForExit(30000); // Wait up to 30 seconds
            }
            catch (Exception ex)
            {
                this.updateMessageAction($"Error stopping server: {ex.Message}");
            }
        }

        // Give a moment for the process to fully terminate
        Thread.Sleep(2000);
        this.updateMessageAction("Server stopped.");
    }

    private void InstallUpdate(string zipPath)
    {
        // Extract to a temporary directory first
        string extractPath = Path.Combine(Path.GetTempPath(), $"godot_server_extract_{Guid.NewGuid()}");
        Directory.CreateDirectory(extractPath);

        ZipFile.ExtractToDirectory(zipPath, extractPath);

        // If GitHub zip contains a root directory, we need to account for that
        string[] topLevelDirs = Directory.GetDirectories(extractPath);
        string sourceDir = extractPath;

        if (topLevelDirs.Length == 1)
        {
            // GitHub zip typically has a single directory at the top level
            sourceDir = topLevelDirs[0];
        }

        // Copy files to server directory
        foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            string relativePath = file.Substring(sourceDir.Length + 1);
            string targetPath = Path.Combine(this.serverInstallPath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            File.Copy(file, targetPath, true);
        }

        // Clean up temp files
        try
        {
            File.Delete(zipPath);
            Directory.Delete(extractPath, true);
        }
        catch (Exception ex)
        {
            this.updateMessageAction($"Warning: Could not clean up temporary files: {ex.Message}");
        }

        this.updateMessageAction("Update installed.");
    }

    private void RestoreFromBackup()
    {
        foreach (string file in Directory.GetFiles(this.backupPath, "*", SearchOption.AllDirectories))
        {
            string relativePath = file[(this.backupPath.Length + 1) ..];
            string targetPath = Path.Combine(this.serverInstallPath, relativePath);
            string? directoryName = Path.GetDirectoryName(targetPath);
            if (string.IsNullOrEmpty(directoryName))
            {
                this.updateMessageAction("Warning: Target path is null or empty. Skipping file restoration.");
                continue;
            }

            Directory.CreateDirectory(directoryName);
            File.Copy(file, targetPath, true);
        }
    }

    private void StartServer()
    {
        // Adjust this process start info for your specific server setup
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(this.serverInstallPath, "godot_server"),
            WorkingDirectory = this.serverInstallPath,
            Arguments = "--headless", // Add any additional arguments your server needs
            UseShellExecute = false,
        };

        Process.Start(startInfo);
        this.updateMessageAction("Server started.");
    }
}
