param location string = resourceGroup().location

param acrName string = 'sleeklearningplatformacr'

param serviceBusNamespaceName string = 'sb-learningplatform'
param serviceBusTopicName string = 'enrollment-created'
param serviceBusSubscriptionName string = 'learning-progress'

param apiIdentityName string = 'uami-learningplatform-api-aks'
param outboxIdentityName string = 'uami-learningplatform-outbox-aks'
param progressIdentityName string = 'uami-learningplatform-progress-aks'
param githubActionsIdentityName string = 'uami-learningplatform-github-actions'

param kubernetesNamespace string = 'learning-platform'
param apiServiceAccountName string = 'sa-learningplatform-api'
param outboxServiceAccountName string = 'sa-learningplatform-outbox'
param progressServiceAccountName string = 'sa-learningplatform-progress'

param githubOidcIssuer string = 'https://token.actions.githubusercontent.com'
param githubOidcSubject string = 'repo:lennilobel@40524895/gspt-learning-platform@1407884216:ref:refs/heads/main'

param aksName string = 'aks-learningplatform'
param aksKubernetesVersion string = '1.35'
param aksDnsPrefix string = 'aks-learni-demos-rg-6327c5'
param aksNodeCount int = 1
param aksNodeVmSize string = 'Standard_D4ds_v5'
param aksAdminUsername string = 'azureuser'
param aksSshPublicKey string = 'ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQCZ16Q+cDPrl+JSZG1qe8hmZ9DJSDxWS+fwn77qlIdsKfD4w0F0SxR4gvuNLLOkIYA7QwLvbqHy3K9iSoTo+C8Gn2hCmaQF72HtUtz71Z+Q7UEuO5hSZzD42thk6cuR0B6N3/3ByxmMjKw8YsnJMSXAGsh/iGlJ2BsmAySiVE7qsf5cQD53ImiZ/81Xg5/Vf1v92qwNn+j94zJwATIMIvdHBKhf7uHULYAGU7pY9K3A0PlZzyLdg18fL6JCQGm10EcH8P1zBQqoAQys+LgNX5kK0rga8VIERI6riQYCkAStW45Zs983Srcuk7OZdsAAYg0rnb58ZnzpHAFVUXHgBXF9'
param aksNodeResourceGroupName string = 'MC_demos-rg_aks-learningplatform_centralus'

param appServicePlanName string = 'asp-learningplatform'
param webAppName string = 'app-learningplatform-api'
param appInsightsName string = 'app-learningplatform-api'
param entraClientId string = '964b4c3a-58d4-4511-a180-06b97dba72eb'
param appInsightsWorkspaceResourceGroupName string = 'DefaultResourceGroup-CUS'
param appInsightsWorkspaceName string = 'DefaultWorkspace-6327c571-12f0-4634-9d3e-404ad37e6be9-CUS'

param functionAppName string = 'func-learningplatform-learningprogress'
param functionPlanName string = 'ASP-demosrg-a6a8'
param functionIdentityName string = 'func-learningplatform-learningprogress-uami'
param functionStorageAccountName string = 'stlearningplatformfunc'
param functionDeploymentContainerName string = 'app-package-func-learningplatform-learningpro-019f508'
param functionAppInsightsName string = 'func-learningplatform-learningprogress'

param sqlServerName string = 'lennidemo'
param sqlDatabaseName string = 'LearningPlatform'

param logAnalyticsWorkspaceName string = 'law-learningplatform'
param containerAppsEnvironmentName string = 'cae-learningplatform'
param apiContainerAppName string = 'ca-learningplatform-api'
param outboxContainerAppName string = 'ca-learningplatform-outbox'
param progressContainerAppName string = 'ca-learningplatform-progress'

@secure()
param learningPlatformConnectionString string

module serviceBus 'modules/service-bus.bicep' = {
  name: 'service-bus'
  params: {
    location: location
    serviceBusNamespaceName: serviceBusNamespaceName
    serviceBusTopicName: serviceBusTopicName
    serviceBusSubscriptionName: serviceBusSubscriptionName
  }
}

module aks 'modules/aks.bicep' = {
  name: 'aks'
  params: {
    location: location
    aksName: aksName
    aksKubernetesVersion: aksKubernetesVersion
    aksDnsPrefix: aksDnsPrefix
    aksNodeCount: aksNodeCount
    aksNodeVmSize: aksNodeVmSize
    aksAdminUsername: aksAdminUsername
    aksSshPublicKey: aksSshPublicKey
    aksNodeResourceGroupName: aksNodeResourceGroupName
  }
}

module identities 'modules/identities.bicep' = {
  name: 'identities'
  params: {
    location: location
    apiIdentityName: apiIdentityName
    outboxIdentityName: outboxIdentityName
    progressIdentityName: progressIdentityName
    githubActionsIdentityName: githubActionsIdentityName
    aksOidcIssuer: aks.outputs.oidcIssuer
    aksName: aks.outputs.clusterName
    serviceBusNamespaceName: serviceBus.outputs.namespaceName
    serviceBusTopicName: serviceBus.outputs.topicName
    kubernetesNamespace: kubernetesNamespace
    apiServiceAccountName: apiServiceAccountName
    outboxServiceAccountName: outboxServiceAccountName
    progressServiceAccountName: progressServiceAccountName
    githubOidcIssuer: githubOidcIssuer
    githubOidcSubject: githubOidcSubject
  }
}

module containerRegistry 'modules/container-registry.bicep' = {
  name: 'container-registry'
  params: {
    location: location
    acrName: acrName
    githubActionsPrincipalId: identities.outputs.githubActionsPrincipalId
    aksKubeletPrincipalId: aks.outputs.kubeletPrincipalId
  }
}

module appService 'modules/app-service.bicep' = {
  name: 'app-service'
  params: {
    location: location
    appServicePlanName: appServicePlanName
    webAppName: webAppName
    appInsightsName: appInsightsName
    entraClientId: entraClientId
    learningPlatformConnectionString: learningPlatformConnectionString
    appInsightsWorkspaceResourceGroupName: appInsightsWorkspaceResourceGroupName
    appInsightsWorkspaceName: appInsightsWorkspaceName
    serviceBusNamespaceName: serviceBus.outputs.namespaceName
    serviceBusTopicName: serviceBus.outputs.topicName
    serviceBusSubscriptionName: serviceBus.outputs.subscriptionName
  }
}

module functionApp 'modules/function.bicep' = {
  name: 'function'
  params: {
    location: location
    functionAppName: functionAppName
    functionPlanName: functionPlanName
    functionIdentityName: functionIdentityName
    functionStorageAccountName: functionStorageAccountName
    functionDeploymentContainerName: functionDeploymentContainerName
    functionAppInsightsName: functionAppInsightsName
    serviceBusNamespaceName: serviceBus.outputs.namespaceName
    sqlServerFqdn: '${sqlServerName}${environment().suffixes.sqlServerHostname}'
    sqlDatabaseName: sqlDatabaseName
  }
}

module containerApps 'modules/container-apps.bicep' = {
  name: 'container-apps'
  params: {
    location: location
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
    containerAppsEnvironmentName: containerAppsEnvironmentName
    apiContainerAppName: apiContainerAppName
    outboxContainerAppName: outboxContainerAppName
    progressContainerAppName: progressContainerAppName
    acrLoginServer: containerRegistry.outputs.acrLoginServer
    serviceBusNamespaceName: serviceBus.outputs.namespaceName
    serviceBusTopicName: serviceBus.outputs.topicName
    serviceBusSubscriptionName: serviceBus.outputs.subscriptionName
    entraClientId: entraClientId
    learningPlatformConnectionString: learningPlatformConnectionString
  }
}

output acrLoginServer string = containerRegistry.outputs.acrLoginServer
output aksOidcIssuer string = aks.outputs.oidcIssuer
output apiIdentityClientId string = identities.outputs.apiClientId
output outboxIdentityClientId string = identities.outputs.outboxClientId
output progressIdentityClientId string = identities.outputs.progressClientId
output githubActionsIdentityClientId string = identities.outputs.githubActionsClientId
output webAppResourceId string = appService.outputs.webAppId
output functionAppResourceId string = functionApp.outputs.functionAppId
output functionIdentityClientId string = functionApp.outputs.functionIdentityClientId
output containerAppsEnvironmentId string = containerApps.outputs.environmentId
output apiContainerAppId string = containerApps.outputs.apiContainerAppId
output outboxContainerAppId string = containerApps.outputs.outboxContainerAppId
output progressContainerAppId string = containerApps.outputs.progressContainerAppId
