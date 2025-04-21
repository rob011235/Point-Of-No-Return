// -----------------------------------------------------------------------
// <copyright file="GameServerManager.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturnServerDeploy
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Godot;
    using PointOfNoReturn.Data.Models;
    using Renci.SshNet;

    /// <summary>
    /// This class is responsible for managing the deployment of a game server.
    /// It handles the connection to a remote server using SFTP and SSH.
    /// It allows for uploading files, setting permissions, and starting the server.
    /// </summary>
    public class GameServerManager
    {
        private readonly Func<string, Task> updateStatus = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameServerManager"/> class.
        /// This constructor sets the location, port, relative path, and update status function for the server manager.
        /// It also initializes the SFTP and SSH clients for connecting to the remote server.
        /// </summary>
        /// <param name="location">Location class object with information on the new location to create a server for.</param>
        /// <param name="port">Port used to access the virtual machine.</param>
        /// <param name="relativePath">Relative path on the server where the dedicated server code resides.</param>
        /// <param name="updateStatus">A method for GameServerManager to call to update staus.</param>
        public GameServerManager(Location location, int port, string relativePath, Func<string, Task>? updateStatus = null)
        {
            this.Location = location;
            this.Port = port;
            this.RelativePath = relativePath;
            if (updateStatus == null)
            {
                this.updateStatus = (s) => Task.Run(() => GD.Print(s));
            }
            else
            {
                this.updateStatus = updateStatus;
            }
        }

        /// <summary>
        /// Gets or sets the location of the server.
        /// This property holds the details of the server's location, including domain name, user name, and password.
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// Gets or sets the port number for the server connection.
        /// This property is used to specify the port number for the SFTP and SSH connections.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the relative path for the server files.
        /// This property is used to specify the path where the server files are found in the application resources.
        /// This property also identifies the path where the files will be uploaded on the remote server.
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// Deploys the server to the specified location.
        /// This method connects to the remote server using SFTP and SSH.
        /// It uploads the server files, sets the necessary permissions, and starts the server.
        /// </summary>
        /// <returns>An async Task.</returns>
        /// <exception cref="ArgumentException">Throws an Argument Exception if DomainName, UserName or Password are missing.</exception>
        public async Task DeployAsync()
        {
            if (this.Location.DomainName == null || this.Location.UserName == null || this.Location.Password == null)
            {
                throw new ArgumentException();
            }

            await this.updateStatus($"Deploying server to {this.Location.DomainName}:{this.Port}");
            using (var client = new SftpClient(this.Location.DomainName, this.Port, this.Location.UserName, this.Location.Password))
            {
                client.Connect();
                await this.updateStatus($"Connected to {this.Location.DomainName}:{this.Port} bginning upload.");
                await this.SftpToDirectory(client, this.RelativePath);

                client.Disconnect();
                client.Dispose();
            }

            using (SshClient sshclient = new SshClient(this.Location.DomainName, this.Location.UserName, this.Location.Password))
            {
                // Enable execute for the server
                sshclient.Connect();
                SshCommand sc = sshclient.CreateCommand($"chmod +x /home/{this.Location.UserName}/{this.RelativePath}/*");
                await sc.ExecuteAsync();
                await this.updateStatus($"Enable server execution result: {sc.Result}.");

                // SshCommand runCommand = sshclient.CreateCommand($" /home/{Location.UserName}/{RelativePath}/PNRServerLynux.sh > foo.out 2> foo.err < /dev/null &");
                SshCommand runCommand = sshclient.CreateCommand($" /home/{this.Location.UserName}/{this.RelativePath}/PNRServerLynux.x86_64 > pnr.out 2> pnr.err < /dev/null &");
                await runCommand.ExecuteAsync();
                await this.updateStatus($"Start server result: {runCommand.Result}.");

                sshclient.Disconnect();
                sshclient.Dispose();
            }
        }

        /// <summary>
        /// Uploads files from the local directory to the remote server using SFTP.
        /// This method recursively uploads all files and directories from the specified local directory to the remote server.
        /// It creates the necessary directories on the remote server if they do not exist.
        /// The method uses the SftpClient to upload files and directories.
        /// The method also updates the status of the upload process using the provided updateStatus function.
        /// The method is asynchronous and returns a Task.
        /// It is important to ensure that the SFTP client is properly disposed of after use.
        /// </summary>
        /// <param name="sftpClient">SFTPClient used to do the upload.</param>
        /// <param name="relativePath">Relative path to the directory currently being uploaded.</param>
        /// <returns>An async Task.</returns>
        public async Task SftpToDirectory(SftpClient sftpClient, string relativePath)
        {
            var localPath = "res://" + relativePath;
            var destPath = sftpClient.WorkingDirectory + "/" + relativePath;

            using var localDir = DirAccess.Open(localPath);
            if (localDir != null)
            {
                // Make the directory if it doesn't exist on the destination server
                try
                {
                    await sftpClient.CreateDirectoryAsync(destPath);
                    await this.updateStatus($"Created directory {destPath}");
                }
                catch (Exception exc)
                {
                    await this.updateStatus($"Could not create {destPath}. {exc.Message} it may have already been created.");
                }

                localDir.ListDirBegin();
                string fileName = localDir.GetNext();
                while (fileName != string.Empty)
                {
                    if (localDir.CurrentIsDir())
                    {
                        string newRelativePath = relativePath + "/" + fileName;
                        await this.SftpToDirectory(sftpClient, newRelativePath);
                    }
                    else
                    {
                        using (var stream = new FileAccessStream(localPath + "/" + fileName, Godot.FileAccess.ModeFlags.Read))
                        {
                            string destFilePath = $"{destPath}/{fileName}";
                            try
                            {
                                // sftpClient.UploadFile(stream, destFilePath, true);
                                var result = sftpClient.BeginUploadFile(stream, destFilePath);
                                while (!result.IsCompleted)
                                {
                                    await Task.Delay(100); // Polling for completion
                                }

                                await this.updateStatus($"Uploaded file {destFilePath}");
                            }
                            catch (Exception exc)
                            {
                                await this.updateStatus($"Could not create {destFilePath}. {exc.Message}");
                            }
                        }
                    }

                    fileName = localDir.GetNext();
                }
            }
            else
            {
                await this.updateStatus("An error occurred when trying to access the path.");
            }
        }

        /// <summary>
        /// Downloads a file from the server.
        /// </summary>
        /// <param name="remoteFilePath">Remote path to download the file from.</param>
        /// <param name="localFilePath">Local path to download the file to.</param>
        /// <returns>An async Task.</returns>
        public async Task DownloadFileAsync(string remoteFilePath = "", string localFilePath = "")
        {
            try
            {
                if (this.Location.DomainName == null || this.Location.UserName == null || this.Location.Password == null)
                {
                    await this.updateStatus("DomainName, UserName, and Password must not be null.");
                    return;
                }

                using var sftpClient = new SftpClient(this.Location.DomainName, this.Port, this.Location.UserName, this.Location.Password);
                sftpClient.Connect();
                await this.updateStatus($"Connected to {this.Location.DomainName}:{this.Port} downloading {remoteFilePath}.");
                using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                {
                    sftpClient.DownloadFile(remoteFilePath, fileStream);
                }

                sftpClient.Disconnect();
            }
            catch (Exception ex)
            {
                await this.updateStatus($"Failed to download file. {ex.Message}");
            }
        }

        /// <summary>
        /// Uploads a single file from the client to the server.
        /// </summary>
        /// <param name="localFilePath">The full path of the file on the client machine.</param>
        /// <param name="remoteFilePath">The full path where the file should be uploaded on the server.</param>
        /// <returns>An async Task.</returns>
        public async Task UploadFileAsync(string localFilePath, string remoteFilePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(localFilePath) || string.IsNullOrWhiteSpace(remoteFilePath))
                {
                    await this.updateStatus("Local file path and remote file path cannot be null or empty.");
                    return;
                }

                if (this.Location.DomainName == null || this.Location.UserName == null || this.Location.Password == null)
                {
                    await this.updateStatus("DomainName, UserName, and Password must not be null.");
                    return;
                }

                using var sftpClient = new SftpClient(this.Location.DomainName, this.Port, this.Location.UserName, this.Location.Password);
                try
                {
                    sftpClient.Connect();
                    await this.updateStatus($"Connected to {this.Location.DomainName}:{this.Port} uploading {localFilePath}.");

                    using (var fileStream = new FileStream(localFilePath, FileMode.Open, System.IO.FileAccess.Read))
                    {
                        sftpClient.UploadFile(fileStream, remoteFilePath, true);
                    }

                    await this.updateStatus($"Successfully uploaded {localFilePath} to {remoteFilePath}.");
                }
                catch (Exception ex)
                {
                    await this.updateStatus($"Failed to upload file. {ex.Message}");
                }
                finally
                {
                    sftpClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                await this.updateStatus($"Failed to upload file. {ex.Message}");
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <returns>An async Task.</returns>
        internal async Task StopServerAsync()
        {
            if (this.Location.DomainName == null || this.Location.UserName == null || this.Location.Password == null)
            {
                await this.updateStatus("DomainName, UserName, and Password must not be null.");
                return;
            }

            using var sshClient = new SshClient(this.Location.DomainName, this.Location.UserName, this.Location.Password);
            try
            {
                sshClient.Connect();
                await this.updateStatus($"Connected to {this.Location.DomainName}:{this.Port} to stop the server.");

                // Send the command to stop the server
                SshCommand stopCommand = sshClient.CreateCommand($"pkill -f /home/{this.Location.UserName}/{this.RelativePath}/PNRServerLynux.x86_64");
                await stopCommand.ExecuteAsync();
                await this.updateStatus($"Stop server result: {stopCommand.Result}.");
            }
            catch (Exception ex)
            {
                await this.updateStatus($"Failed to stop the server. {ex.Message}");
            }
            finally
            {
                sshClient.Disconnect();
            }
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <returns>An async Task.</returns>
        internal async Task StartServerAsync()
        {
            if (this.Location.DomainName == null || this.Location.UserName == null || this.Location.Password == null)
            {
            await this.updateStatus("DomainName, UserName, and Password must not be null.");
            return;
            }

            using var sshClient = new SshClient(this.Location.DomainName, this.Location.UserName, this.Location.Password);
            try
            {
                sshClient.Connect();
                await this.updateStatus($"Connected to {this.Location.DomainName}:{this.Port} to start the server.");

                // Send the command to start the server
                SshCommand startCommand = sshClient.CreateCommand($"nohup /home/{this.Location.UserName}/{this.RelativePath}/PNRServerLynux.x86_64 > pnr.out 2> pnr.err < /dev/null &");
                await startCommand.ExecuteAsync();
                await this.updateStatus($"Start server result: {startCommand.Result}.");
            }
            catch (Exception ex)
            {
                await this.updateStatus($"Failed to start the server. {ex.Message}");
            }
            finally
            {
                sshClient.Disconnect();
            }
        }

        /// <summary>
        /// Updates the server credentials.
        /// </summary>
        /// <param name="oldLocationData">Old location data.</param>
        /// <param name="newLocationData">New location data.</param>
        /// <returns>An async Task.</returns>
        internal async Task UpdateServerCredentialsAsync(Location oldLocationData, Location newLocationData)
        {
            GD.Print("Updating server credentials.");
            if (oldLocationData.DomainName == null || oldLocationData.UserName == null || oldLocationData.Password == null)
            {
                await this.updateStatus("DomainName, UserName, and Password must not be null.");
                return;
            }

            GD.Print("TODO: Implement updating server credentials.");
        }
    }
}
