using System;
using Azure.ResourceManager.Compute;

namespace PointOfNoReturnServerDeploy
{
    /// <summary>
    /// This class represents the result of a virtual machine (VM) operation.
    /// It contains information about the VM resource, domain name, success status,
    /// message, and any exceptions that occurred during the operation.
    /// </summary>
    public class VMResult
    {
        /// <summary>
        /// Gets or sets the virtual machine resource.
        /// This property holds the details of the VM that was created or managed.
        /// </summary>
        public VirtualMachineResource? VirtualMachineResource { get; set; }
        /// <summary>
        /// Gets or sets the domain name associated with the virtual machine.
        /// This property is used to identify the VM's network address.
        /// It may be null if the VM was not created successfully or if no domain name is assigned.
        /// </summary>
        public string? DomainName { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// This property is used to indicate the success or failure of the VM operation.
        /// A value of true indicates that the operation was successful, while false indicates failure.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Gets or sets the message associated with the operation.
        /// This property can be used to provide additional information about the operation's result.
        /// It may contain success messages, error messages, or other relevant information.
        /// The message can be null if no specific message is provided.
        /// </summary>
        public string? Message { get; set; }
        /// <summary>
        /// Gets or sets the exception that occurred during the operation.
        /// This property is used to capture any errors or exceptions that were thrown during the VM operation.
        /// It can be null if no exceptions occurred.
        /// This property is useful for debugging and error handling purposes.
        /// </summary>
        public Exception? Exception { get; set; }
        /// <summary>
        /// Provides a string representation of the VMResult object.
        /// </summary>
        public override string ToString()
        {
            return $"Success: {this.Success}\n{this.Message}";
        }
    }
}