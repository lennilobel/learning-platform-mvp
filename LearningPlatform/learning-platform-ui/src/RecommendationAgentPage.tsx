import { useMsal } from '@azure/msal-react'
import { useState, type ChangeEvent } from 'react'
import './App.css'
import { loginRequest } from './authConfig'
import ReactMarkdown from 'react-markdown'

type RecommendationAgentResponse = {
    response: string
}

function RecommendationAgentPage(): React.JSX.Element {
    const { instance, accounts } = useMsal()
    const [prompt, setPrompt] = useState<string>(
        'For learner 1, recommend the best two courses and explain why.'
    )
    const [response, setResponse] = useState<string>('')
    const [isLoading, setIsLoading] = useState<boolean>(false)

    function promptChanged(event: ChangeEvent<HTMLTextAreaElement>): void {
        setPrompt(event.target.value)
    }

    async function submitPrompt(event: React.SubmitEvent<HTMLFormElement>): Promise<void> {
        event.preventDefault()
        if (prompt.trim() === '') {
            return
        }
        setIsLoading(true)
        setResponse('')
        try {
            const result = await runRecommendationAgent(prompt)
            setResponse(result.response)
        }
        finally {
            setIsLoading(false)
        }
    }

    // --- recommendation agent service

    async function runRecommendationAgent(prompt: string): Promise<RecommendationAgentResponse> {
        const tokenResponse = await instance.acquireTokenSilent({
            ...loginRequest,
            account: accounts[0]
        })
        const response = await fetch(
            '/api/recommendation-agent',
            {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${tokenResponse.accessToken}`
                },
                body: JSON.stringify({
                    prompt
                })
            }
        )
        if (!response.ok) {
            alert('Failure response returned by API')
            console.error('Failure response returned by API', response)
            throw new Error('Failure response returned by API')
        }
        const result: RecommendationAgentResponse = await response.json()
        return result
    }

    return (
        <>
            <h1>Recommendation Agent</h1>
            <form onSubmit={submitPrompt}>
                <p>
                    <textarea
                        rows={6}
                        cols={80}
                        value={prompt}
                        onChange={promptChanged}
                    />
                </p>
                <button type="submit" disabled={isLoading}>
                    {isLoading ? 'Thinking...' : 'Ask Agent'}
                </button>
            </form>
            {response &&
                <>
                    <h2>Agent Response</h2>
                    <div className='agent-response'>
                        <ReactMarkdown>
                            {response}
                        </ReactMarkdown>
                    </div>
                </>
            }
        </>
    )
}

export default RecommendationAgentPage
