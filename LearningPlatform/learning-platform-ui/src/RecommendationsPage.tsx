import { useMsal } from '@azure/msal-react'
import { useState, type ChangeEvent } from 'react'
import './App.css'
import { loginRequest } from './authConfig'

type RecommendationRequest = {
    learnerId: number | null
    maxResults: number | null
}

type Course = {
    courseId: number
    title: string
    role: string
    isActive: boolean
    prerequisiteCourseIds: number[]
    skills: string[]
}

type RankedCourse = {
    course: Course
    score: number
    reasons: string[]
}

type ValidationErrors = {
    learnerId?: string
    maxResults?: string
}

function RecommendationsPage(): React.JSX.Element {
    const { instance, accounts } = useMsal()
    const [request, setRequest] = useState<RecommendationRequest>({
        learnerId: 1,
        maxResults: 2
    })
    const [recommendations, setRecommendations] = useState<RankedCourse[]>([])
    const [isLoading, setIsLoading] = useState<boolean>(false)
    const [validationErrors, setValidationErrors] = useState<ValidationErrors>({})

    function learnerIdChanged(event: ChangeEvent<HTMLInputElement>): void {
        setRequest({
            ...request,
            learnerId: event.target.value === '' ? null : Number(event.target.value)
        })
        setValidationErrors({
            ...validationErrors,
            learnerId: undefined
        })
    }

    function maxResultsChanged(event: ChangeEvent<HTMLInputElement>): void {
        setRequest({
            ...request,
            maxResults: event.target.value === '' ? null : Number(event.target.value)
        })
        setValidationErrors({
            ...validationErrors,
            maxResults: undefined
        })
    }

    function validateRequest(): ValidationErrors {
        const errors: ValidationErrors = {}
        if (request.learnerId === null || request.learnerId < 1) {
            errors.learnerId = 'Learner ID must be 1 or higher.'
        }
        if (request.maxResults === null || request.maxResults < 1) {
            errors.maxResults = 'Maximum results must be 1 or higher.'
        }
        return errors
    }

    async function submitRequest(event: React.SubmitEvent<HTMLFormElement>): Promise<void> {
        event.preventDefault()
        const errors = validateRequest()
        setValidationErrors(errors)
        if (Object.keys(errors).length > 0) {
            return
        }
        setIsLoading(true)
        setRecommendations([])
        try {
            const result = await getRecommendations(
                request.learnerId!,
                request.maxResults!
            )
            setRecommendations(result)
        }
        finally {
            setIsLoading(false)
        }
    }

    // --- recommendation service

    async function getRecommendations(learnerId: number, maxResults: number): Promise<RankedCourse[]> {
        const tokenResponse = await instance.acquireTokenSilent({
            ...loginRequest,
            account: accounts[0]
        })
        const response = await fetch(
            `/api/learners/${learnerId}/recommendations?maxResults=${maxResults}`,
            {
                headers: {
                    'Authorization': `Bearer ${tokenResponse.accessToken}`
                }
            }
        )
        if (response.status === 404) {
            setValidationErrors({
                learnerId: `Learner ${learnerId} was not found.`
            })
            return []
        }
        if (!response.ok) {
            alert('Failure response returned by API')
            console.error('Failure response returned by API', response)
            throw new Error('Failure response returned by API')
        }
        const result: RankedCourse[] = await response.json()
        return result
    }

    return (
        <>
            <h1>Course Recommendations</h1>
            <form onSubmit={submitRequest}>
                <table>
                    <tbody>
                        <tr>
                            <td>
                                Learner ID:
                            </td>
                            <td>
                                <input
                                    type="number"
                                    value={request.learnerId ?? ''}
                                    onChange={learnerIdChanged}
                                />
                            </td>
                            <td className='validation-error'>
                                {validationErrors.learnerId
                                    ? <span>{validationErrors.learnerId}</span>
                                    : null}
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Maximum results:
                            </td>
                            <td>
                                <input
                                    type="number"
                                    value={request.maxResults ?? ''}
                                    onChange={maxResultsChanged}
                                />
                            </td>
                            <td className='validation-error'>
                                {validationErrors.maxResults
                                    ? <span>{validationErrors.maxResults}</span>
                                    : null}
                            </td>
                        </tr>
                    </tbody>
                </table>
                <button type="submit" disabled={isLoading}>
                    {isLoading ? 'Loading...' : 'Get Recommendations'}
                </button>
            </form>
            {recommendations.map(recommendation =>
                <div key={recommendation.course.courseId}>
                    <h2>{recommendation.course.title}</h2>
                    <p>
                        Course ID: {recommendation.course.courseId}
                    </p>
                    <p>
                        Role: {recommendation.course.role}
                    </p>
                    <p>
                        Score: {recommendation.score}
                    </p>
                    <p>
                        Prerequisites: {
                            recommendation.course.prerequisiteCourseIds.length === 0
                                ? 'None'
                                : recommendation.course.prerequisiteCourseIds.join(', ')
                        }
                    </p>
                    <p>
                        Skills: {recommendation.course.skills.join(', ')}
                    </p>
                    <p>Reasons:</p>
                    <ul>
                        {recommendation.reasons.map(reason =>
                            <li key={reason}>{reason}</li>
                        )}
                    </ul>
                </div>
            )}
        </>
    )
}

export default RecommendationsPage
