param location string
param aksName string
param aksKubernetesVersion string
param aksDnsPrefix string
param aksNodeCount int
param aksNodeVmSize string
param aksAdminUsername string
param aksSshPublicKey string
param aksNodeResourceGroupName string

// Define the AKS cluster
resource aks 'Microsoft.ContainerService/managedClusters@2026-05-01' = {
  name: aksName
  location: location
  kind: 'Base'
  sku: {
    name: 'Base'
    tier: 'Free'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    kubernetesVersion: aksKubernetesVersion
    dnsPrefix: aksDnsPrefix
    agentPoolProfiles: [
      {
        name: 'nodepool1'
        count: aksNodeCount
        vmSize: aksNodeVmSize
        mode: 'System'
        osType: 'Linux'
        osDiskType: 'Ephemeral'
        maxPods: 250
        enableAutoScaling: false
        osDiskSizeGB: 150
        osSKU: 'Ubuntu'
        kubeletDiskType: 'OS'
        type: 'VirtualMachineScaleSets'
        securityProfile: {
          enableSecureBoot: false
          enableVTPM: false
          sshAccess: 'LocalUser'
        }
        upgradeSettings: {
          maxSurge: '10%'
          maxUnavailable: '0'
        }
        enableEncryptionAtHost: false
        enableFIPS: false
        enableNodePublicIP: false
        enableUltraSSD: false
        scaleDownMode: 'Delete'
      }
    ]
    networkProfile: {
      networkPlugin: 'azure'
      networkPluginMode: 'overlay'
      networkDataplane: 'azure'
      networkPolicy: 'none'
      loadBalancerSku: 'standard'
      outboundType: 'loadBalancer'
      podCidr: '10.244.0.0/16'
      serviceCidr: '10.0.0.0/16'
      dnsServiceIP: '10.0.0.10'
      ipFamilies: [
        'IPv4'
      ]
      loadBalancerProfile: {
        managedOutboundIPs: {
          count: 1
        }
      }
    }
    oidcIssuerProfile: {
      enabled: true
    }
    securityProfile: {
      workloadIdentity: {
        enabled: true
      }
    }
    enableRBAC: true
    autoUpgradeProfile: {
      nodeOSUpgradeChannel: 'NodeImage'
    }
    nodeProvisioningProfile: {
      mode: 'Manual'
    }
    hostedSystemProfile: {
      enabled: false
    }
    storageProfile: {
      diskCSIDriver: {
        enabled: true
      }
      fileCSIDriver: {
        enabled: true
      }
      snapshotController: {
        enabled: true
      }
    }
    supportPlan: 'KubernetesOfficial'
    linuxProfile: {
      adminUsername: aksAdminUsername
      ssh: {
        publicKeys: [
          {
            keyData: aksSshPublicKey
          }
        ]
      }
    }
    nodeResourceGroup: aksNodeResourceGroupName
    metricsProfile: {
      costAnalysis: {
        enabled: false
      }
    }
    servicePrincipalProfile: {
      clientId: 'msi'
    }
  }
}

// Return values needed by other modules
output aksId string = aks.id
output clusterName string = aks.name
output oidcIssuer string = aks.properties.oidcIssuerProfile.issuerURL
output kubeletPrincipalId string = aks.properties.identityProfile.kubeletidentity.objectId
