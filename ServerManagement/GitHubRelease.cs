// -----------------------------------------------------------------------
// <copyright file="GitHubRelease.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

/// <summary>
/// Represents a GitHub release.
/// </summary>
public class GitHubRelease
{
    /// <summary>
    /// gets or sets the domain name for the release.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// gets or sets the domain name for the release.
    /// </summary>
    public string? HtmlUrl { get; set; }

    /// <summary>
    /// Gets or sets the asset domain name for the release.
    /// </summary>
    public string? AssetsUrl { get; set; }

    /// <summary>
    /// Gets or sets the upload URL for the release.
    /// </summary>
    public string? UploadUrl { get; set; }

    /// <summary>
    /// Gets or sets the tag name for the release.
    /// </summary>
    public string? TagName { get; set; }

    /// <summary>
    /// Gets or sets the Name for the release.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a draft.
    /// </summary>
    public bool Draft { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether release is a pre-release.
    /// </summary>
    public bool Prerelease { get; set; }

    /// <summary>
    /// Gets or sets where the release was created at.
    /// </summary>
    public string? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets where the release was published at.
    /// </summary>
    public string? PublishedAt { get; set; }

    /// <summary>
    /// Gets or sets the list of assets for the release.
    /// </summary>
    public List<GitHubAsset>? Assets { get; set; }

    /// <summary>
    /// Gets or sets the Tarball URL for the release.
    /// </summary>
    public string? TarballUrl { get; set; }

    /// <summary>
    /// Gets or sets the zipball URL for the release.
    /// </summary>
    public string? ZipballUrl { get; set; }

    /// <summary>
    /// Gets or sets the body of the release.
    /// </summary>
    public string? Body { get; set; }
}
