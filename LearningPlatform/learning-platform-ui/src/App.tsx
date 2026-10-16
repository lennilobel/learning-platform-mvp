import { useMsal } from '@azure/msal-react'
import { Link, Route, Routes } from 'react-router'
import './App.css'
import EnrollmentPage from './EnrollmentPage'
import Home from './HomePage'

function App(): React.JSX.Element {
    const { instance, accounts } = useMsal()
    const account = accounts[0]
    return (
        <>
            <nav>
                <Link className='nav-link' to="/">Home</Link>
                <Link className='nav-link' to="/enrollment">Enrollment</Link>
                <span>
                    Signed in as {account?.name ?? account?.username}
                </span>
                <button
                    type="button"
                    onClick={() => instance.logoutRedirect({
                        account,
                        postLogoutRedirectUri: window.location.origin
                    })}
                >
                    Sign out
                </button>
            </nav>
            <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/enrollment" element={<EnrollmentPage />} />
            </Routes>
        </>
    )
}

export default App
