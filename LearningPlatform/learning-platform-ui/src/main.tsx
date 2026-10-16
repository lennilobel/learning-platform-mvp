import { InteractionType, PublicClientApplication } from '@azure/msal-browser'
import { MsalAuthenticationTemplate, MsalProvider } from '@azure/msal-react'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router'
import App from './App.tsx'
import { loginRequest, msalConfig } from './authConfig.ts'
import './index.css'

const msalInstance = new PublicClientApplication(msalConfig)
await msalInstance.initialize()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MsalProvider instance={msalInstance}>
      <MsalAuthenticationTemplate interactionType={InteractionType.Redirect} authenticationRequest={loginRequest}>
        <BrowserRouter>
          <App />
        </BrowserRouter>
      </MsalAuthenticationTemplate>
    </MsalProvider>
  </StrictMode>,
)
