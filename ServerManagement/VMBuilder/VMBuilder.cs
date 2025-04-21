// -----------------------------------------------------------------------
// <copyright file="VMBuilder.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturnServerDeploy
{
    using System;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Core;
    using Azure.Identity;
    using Azure.ResourceManager;
    using Azure.ResourceManager.Compute;
    using Azure.ResourceManager.Compute.Models;
    using Azure.ResourceManager.Network;
    using Azure.ResourceManager.Network.Models;
    using Azure.ResourceManager.Resources;
    using Godot;

    /// <summary>
    /// Class that builds virtual machines.
    /// </summary>
    public class VMBuilder
    {
        private readonly string tenantId; // "ffe4dd42-6573-45fe-895f-8791b3688919"
        private Func<string, Task> updateStatus = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="VMBuilder"/> class.
        /// </summary>
        /// <param name="name">Name of the virtual machine.</param>
        /// <param name="adminName">Name of the admin user.</param>
        /// <param name="adminPassword">Password of the admin user.</param>
        /// <param name="updateStatus">Function to update the status.</param>
        /// <param name="location">Location of the virtual machine.</param>
        /// <param name="tenantId">Tenant ID for Azure authentication.</param>
        public VMBuilder(string name, string adminName, string adminPassword, Func<string, Task>? updateStatus = null, AzureLocation? location = null, string tenantId = "")
        {
            this.Name = System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9-]", string.Empty).Replace(" ", "-").ToLower();
            this.AdminName = adminName;
            this.AdminPassword = adminPassword;
            this.Location = location ?? AzureLocation.WestUS3;
            this.tenantId = tenantId;

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
        /// Gets or sets the name of the virtual machine.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the admin user.
        /// </summary>
        public string AdminName { get; set; }

        /// <summary>
        /// Gets or sets the password of the admin user.
        /// </summary>
        public string AdminPassword { get; set; }

        /// <summary>
        /// Gets or sets the location of the virtual machine.
        /// </summary>
        public AzureLocation Location { get; set; } = AzureLocation.WestUS3;

        /// <summary>
        /// Builds the virtual machine asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the VMResult.</returns>
        public async Task<VMResult> BuildAsync()
        {
            try
            {
                /**
                * Authenticate interactively in the browser.
                */
                InteractiveBrowserCredential credentials = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions()
                {
                    TenantId = this.tenantId,
                });
                ArmClient armClient = new ArmClient(credentials);

                // Check to see if resource group already exists
                var resourceGroupName = this.Name + "-rg";
                var subscription = await armClient.GetDefaultSubscriptionAsync();
                var resourceGroupCollection = subscription.GetResourceGroups();
                var existingResourceGroupResourceResponse = await resourceGroupCollection.GetIfExistsAsync(resourceGroupName);
                if (existingResourceGroupResourceResponse.HasValue)
                {
                    var existingResourceGroupResource = existingResourceGroupResourceResponse.Value;
                    if (existingResourceGroupResource is not null)
                    {
                        await this.updateStatus("Found existing server. Deleting and Recreating.");

                        // TODO: When state is added make sure to backup and restore state when doing this.
                        await existingResourceGroupResource.DeleteAsync(WaitUntil.Completed);
                    }
                }

                // Create Resource Group
                var resourceGroupLRO = await resourceGroupCollection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, resourceGroupName, new ResourceGroupData(this.Location));
                var resourceGroupResource = resourceGroupLRO.Value;
                await this.updateStatus("Created Resource Group");

                // Create Virtual Net
                var vnetName = this.Name + "-vnet";
                VirtualNetworkCollection vNetcollection = resourceGroupResource.GetVirtualNetworks();
                VirtualNetworkData data = new ()
                {
                    Location = this.Location,
                    AddressPrefixes = { "172.16.0.0/16" },
                };
                ArmOperation<VirtualNetworkResource> vnetLro = await vNetcollection.CreateOrUpdateAsync(WaitUntil.Completed, vnetName, data);
                var vnet = vnetLro.Value;
                await this.updateStatus("Created vnet");

                // Create Subnet
                var subnetName = this.Name + "-subnet";
                SubnetCollection subnetCollection = vnet.GetSubnets();
                SubnetData subnetData = new ()
                {
                    AddressPrefix = "172.16.0.0/24",
                };
                ArmOperation<SubnetResource> subnetLro = await subnetCollection.CreateOrUpdateAsync(WaitUntil.Completed, subnetName, subnetData);
                var subnet = subnetLro.Value;
                await this.updateStatus("Created subnet");

                // Create IP Address
                string publicIpAddressName = this.Name + "-ip";
                PublicIPAddressCollection publicIpAddressCollection = resourceGroupResource.GetPublicIPAddresses();
                PublicIPAddressData publicIpAddressData = new ()
                {
                    Location = this.Location,
                    Sku = new PublicIPAddressSku()
                    {
                        Name = PublicIPAddressSkuName.Basic,
                    },
                    PublicIPAllocationMethod = NetworkIPAllocationMethod.Static,
                    PublicIPAddressVersion = NetworkIPVersion.IPv4,
                    DnsSettings = new PublicIPAddressDnsSettings()
                    {
                        DomainNameLabel = this.Name,
                    },
                };
                ArmOperation<PublicIPAddressResource> publicIpAddressLro = await publicIpAddressCollection.CreateOrUpdateAsync(WaitUntil.Completed, publicIpAddressName, publicIpAddressData);
                var publicIpAddress = publicIpAddressLro.Value;
                await this.updateStatus("Created ip address");

                // Create network interface
                string networkInterfaceName = this.Name + "-ni";
                NetworkInterfaceCollection networkInterfaceCollection = resourceGroupResource.GetNetworkInterfaces();

                NetworkInterfaceData networkInterfaceData = new ()
                {
                    Location = this.Location,
                    IPConfigurations =
                    {
                        new NetworkInterfaceIPConfigurationData()
                        {
                            Name = "primary",
                            Primary = true,
                            Subnet = subnet.Data,
                            PublicIPAddress = new PublicIPAddressData()
                            {
                                Id = publicIpAddress.Id,
                            },
                        },
                    },
                    EnableAcceleratedNetworking = true,
                };
                ArmOperation<NetworkInterfaceResource> networkInterfacelro = await networkInterfaceCollection.CreateOrUpdateAsync(WaitUntil.Completed, networkInterfaceName, networkInterfaceData);
                var networkInterface = networkInterfacelro.Value;

                var domainName = $"{this.Name}.{this.Location.Name}.cloudapp.azure.com";
                await this.updateStatus($"Created network interface with domain name {domainName}");

                // Create Virtual Machine
                var vmData = new VirtualMachineData(this.Location)
                {
                    Location = this.Location,
                    StorageProfile = new VirtualMachineStorageProfile()
                    {
                        ImageReference = new ImageReference()
                        {
                            Offer = "ubuntu-24_04-lts",
                            Publisher = "Canonical",
                            Sku = "server",
                            Version = "latest",
                        },
                    },
                    HardwareProfile = new VirtualMachineHardwareProfile()
                    {
                        VmSize = VirtualMachineSizeType.StandardDS1V2,
                    },
                    OSProfile = new VirtualMachineOSProfile()
                    {
                        ComputerName = this.Name,
                        AdminUsername = this.AdminName,
                        AdminPassword = this.AdminPassword,
                    },
                    NetworkProfile = new VirtualMachineNetworkProfile()
                    {
                        NetworkInterfaces =
                       {
                            new VirtualMachineNetworkInterfaceReference()
                            {
                                 Id = networkInterface.Id,
                                 Primary = true,
                            },
                       },
                    },
                    Priority = VirtualMachinePriorityType.Spot,
                    EvictionPolicy = VirtualMachineEvictionPolicyType.Deallocate,
                    BillingMaxPrice = -1,
                };
                var virtualMachines = resourceGroupResource.GetVirtualMachines();

                var vmLro = await virtualMachines.CreateOrUpdateAsync(WaitUntil.Completed, this.Name, vmData);
                VirtualMachineResource vmr = vmLro.Value;

                await this.updateStatus("Created virtual machine");

                return new VMResult()
                {
                    VirtualMachineResource = vmr,
                    DomainName = domainName,
                    Success = true,
                    Message = $"Virtual Machine Created Successfully.",
                };
            }
            catch (Exception exc)
            {
                return new VMResult()
                {
                    VirtualMachineResource = null,
                    DomainName = null,
                    Success = false,
                    Message = exc.Message,
                    Exception = exc,
                };
            }
        }

        /// <summary>
        /// Gets the status of the virtual machine.
        /// </summary>
        /// <returns>The status of the virtual machine.</returns>
        public async Task<string> GetVMStatusAsync()
        {
            string result = string.Empty;

            // Connect to the VM and get the status
            try
            {
                InteractiveBrowserCredential credentials = new (new InteractiveBrowserCredentialOptions()
                {
                    TenantId = "ffe4dd42-6573-45fe-895f-8791b3688919",
                });
                ArmClient armClient = new ArmClient(credentials);

                var subscription = await armClient.GetDefaultSubscriptionAsync();
                var resourceGroupName = this.Name + "-rg";
                var resourceGroup = await subscription.GetResourceGroups().GetAsync(resourceGroupName);

                var virtualMachines = resourceGroup.Value.GetVirtualMachines();
                var vm = await virtualMachines.GetAsync(this.Name);

                var instanceView = vm.Value.Data.InstanceView;
                var statuses = instanceView.Statuses;

                foreach (var status in statuses)
                {
                    if (status.Code.StartsWith("PowerState/"))
                    {
                        result = status.DisplayStatus;
                        break;
                    }
                }
            }
            catch (Exception exc)
            {
                result = $"Error retrieving VM status: {exc.Message}";
            }

            return result;
        }
    }
}
