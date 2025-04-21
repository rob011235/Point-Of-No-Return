using System;
using System.IO;
using FileAccess = Godot.FileAccess;

// Created by Novaleaf Edu.Godot.CSharp
// Source: https://www.reddit.com/r/GodotCSharp/comments/1e49pcm/fileaccessstreamcs_convert_godot_fileaccess_into/
namespace PointOfNoReturnServerDeploy
{
    /// <summary>
    /// Provides a stream implementation that wraps around the Godot FileAccess class.
    /// Supports reading from, writing to, and seeking within a file.
    /// </summary>
    public class FileAccessStream : Stream
    {
        private readonly FileAccess fileAccess;
        private readonly long fileLength;
        private long position;

        public FileAccess.ModeFlags ModeFlags { get; private init; }

        public const int BufferSize = 1048576; // 1MB. Adjust this as needed for performance.

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAccessStream"/> class.
        /// Opens the file at the specified path using the given mode flags.
        /// </summary>
        /// <param name="path">The path to the file to open.</param>
        /// <param name="modeFlags">The mode flags specifying how the file should be opened.</param>
        /// <exception cref="IOException">Thrown when the file cannot be opened.</exception>
        public FileAccessStream(string path, FileAccess.ModeFlags modeFlags)
        {
            this.fileAccess = FileAccess.Open(path, modeFlags) ?? throw new IOException("Failed to open file.");
            this.fileLength = (long)this.fileAccess.GetLength();
            this.position = 0;
            this.ModeFlags = modeFlags;
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// Always returns true because FileAccess supports reading.
        /// </summary>
        public override bool CanRead => (this.ModeFlags & FileAccess.ModeFlags.Read) != 0;

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// Always returns true because FileAccess supports seeking.
        /// </summary>
        public override bool CanSeek => true;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// Returns true if the FileAccess mode includes writing.
        /// </summary>
        public override bool CanWrite => (this.ModeFlags & FileAccess.ModeFlags.Write) != 0;

        /// <summary>
        /// Gets the length of the file stream in bytes.
        /// </summary>
        public override long Length => this.fileLength;

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get => this.position;
            set => this.Seek(value, SeekOrigin.Begin);
        }

        /// <summary>
        /// Flushes any buffered data to the file.
        /// Calls the FileAccess.Flush method.
        /// </summary>
        public override void Flush()
        {
            try
            {
                this.fileAccess.Flush();
            }
            catch (Exception ex)
            {
                throw new IOException("Failed to flush the file.", ex);
            }
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero if the end of the stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
            if (buffer.Length - offset < count) throw new ArgumentException("The buffer is too small.");

            if (this.position >= this.fileLength) return 0;

            int bytesRead = 0;
            try
            {
                while (count > 0)
                {
                    int bytesToRead = (int)Math.Min(count, BufferSize);
                    byte[] chunk = this.fileAccess.GetBuffer(bytesToRead);
                    if (chunk.Length == 0) break;

                    Array.Copy(chunk, 0, buffer, offset, chunk.Length);
                    offset += chunk.Length;
                    count -= chunk.Length;
                    bytesRead += chunk.Length;
                    this.position += chunk.Length;
                }
            }
            catch (Exception ex)
            {
                throw new IOException("Failed to read from the file.", ex);
            }

            return bytesRead;
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPosition = offset;
                    break;
                case SeekOrigin.Current:
                    newPosition = this.position + offset;
                    break;
                case SeekOrigin.End:
                    newPosition = this.fileLength + offset;
                    break;
                default:
                    throw new ArgumentException("Invalid seek origin.", nameof(origin));
            }

            if (newPosition < 0) throw new IOException("Cannot seek to a negative position.");
            if (newPosition > this.fileLength) throw new IOException("Cannot seek beyond the end of the stream.");

            try
            {
                this.fileAccess.Seek((ulong)newPosition);
                this.position = newPosition;
            }
            catch (Exception ex)
            {
                throw new IOException("Failed to seek in the file.", ex);
            }

            return this.position;
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// Always throws NotSupportedException because FileAccessStream does not support setting the length.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
            if (buffer.Length - offset < count) throw new ArgumentException("The buffer is too small.");

            try
            {
                while (count > 0)
                {
                    int bytesToWrite = (int)Math.Min(count, BufferSize);
                    byte[] chunk = new byte[bytesToWrite];
                    Array.Copy(buffer, offset, chunk, 0, bytesToWrite);

                    this.fileAccess.StoreBuffer(chunk);
                    offset += bytesToWrite;
                    count -= bytesToWrite;
                    this.position += bytesToWrite;
                }
            }
            catch (Exception ex)
            {
                throw new IOException("Failed to write to the file.", ex);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FileAccessStream"/> and optionally releases the managed resources.
        /// Closes the file by calling FileAccess.Close().
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.fileAccess?.Close();
            }

            base.Dispose(disposing);
        }
    }
}
