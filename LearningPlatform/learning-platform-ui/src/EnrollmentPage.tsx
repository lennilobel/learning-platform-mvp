import { useMsal } from '@azure/msal-react'
import { useState, type ChangeEvent } from 'react'
import './App.css'
import { loginRequest } from './authConfig'

type Enrollment = {
    learnerId: number | null,
    courseId: number | null
}

type ValidationErrors = {
    learnerId?: string,
    courseId?: string
}

type EnrollmentSubmissionResult = {
    isDuplicate: boolean
    validationErrors: ValidationErrors
}

function EnrollmentPage(): React.JSX.Element {
    const { instance, accounts } = useMsal()
    const [enrollment, setEnrollment] = useState<Enrollment>({
        learnerId: null,
        courseId: null
    })

    const [isSubmitting, setIsSubmitting] = useState<boolean>(false)
    const [isSuccess, setIsSuccess] = useState<boolean>(false)
    const [validationErrors, setValidationErrors] = useState<ValidationErrors>({})

    function learnerIdChanged(event: ChangeEvent<HTMLInputElement>): void {
        setEnrollment({
            ...enrollment,
            learnerId: event.target.value === '' ? null : Number(event.target.value)
        })
        setValidationErrors({
            ...validationErrors,
            learnerId: undefined
        })
    }

    function courseIdChanged(event: ChangeEvent<HTMLInputElement>): void {
        setEnrollment({
            ...enrollment,
            courseId: event.target.value === '' ? null : Number(event.target.value)
        })
        setValidationErrors({
            ...validationErrors,
            courseId: undefined
        })
    }

    function validateEnrollment(): ValidationErrors {
        const errors: ValidationErrors = {}
        if (enrollment.learnerId === null || enrollment.learnerId < 1) {
            errors.learnerId = 'Learner ID must be 1 or higher.'
        }
        if (enrollment.courseId === null || enrollment.courseId < 1) {
            errors.courseId = 'Course ID must be 1 or higher.'
        }
        return errors
    }

    async function submitEnrollment(event: React.SubmitEvent<HTMLFormElement>): Promise<void> {
        event.preventDefault()
        setIsSuccess(false)
        const errors = validateEnrollment()
        setValidationErrors(errors)
        if (Object.keys(errors).length > 0) {
            return
        }
        setIsSubmitting(true)
        try {
            const result = await createEnrollment(enrollment)
            if (result.isDuplicate) {
                alert('Duplicate enrollment')
                return
            }
            if (result.validationErrors.courseId === null && result.validationErrors.learnerId === null) {
                setIsSuccess(true)
            }
            else {
                setValidationErrors(result.validationErrors)
            }
        }
        finally {
            setIsSubmitting(false)
        }
    }

    // --- enrollment service

    async function createEnrollment(enrollment: Enrollment): Promise<EnrollmentSubmissionResult> {
        const tokenResponse = await instance.acquireTokenSilent({
            ...loginRequest,
            account: accounts[0]
        })
        const response = await fetch(
            '/api/enrollments',
            {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${tokenResponse.accessToken}`
                },
                body: JSON.stringify(enrollment)
            }
        )
        if (!response.ok && response.status !== 409) {
            alert('Failure response returned by API')
            console.error('Failure response returned by API', response)
            throw new Error('Failure response returned by API')
        }
        const result: EnrollmentSubmissionResult = await response.json()
        return result
    }

    return (
        <>
            <h1>Submit an Enrollment</h1>
            <form onSubmit={submitEnrollment}>
                <table>
                    <tr>
                        <td>
                            Learner ID:
                        </td>
                        <td>
                            <input
                                type="number"
                                value={enrollment.learnerId ?? ''}
                                onChange={learnerIdChanged}
                            />
                        </td>
                        <td className='validation-error'>
                            {validationErrors.learnerId ? <span>{validationErrors.learnerId}</span> : null}
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Course ID:
                        </td>
                        <td>
                            <input
                                type="number"
                                value={enrollment.courseId ?? ''}
                                onChange={courseIdChanged}
                            />
                        </td>
                        <td className='validation-error'>
                            {validationErrors.courseId ? <span>{validationErrors.courseId}</span> : null}
                        </td>
                    </tr>
                </table>
                <button type="submit" disabled={isSubmitting}>
                    {isSubmitting ? 'Submitting...' : 'Submit Enrollment'}
                </button>
            </form>
            <p>
                {enrollment.learnerId} / {enrollment.courseId}
            </p>
            {isSuccess &&
                <p>Enrollment submitted successfully!</p>
            }
        </>
    )
}

export default EnrollmentPage
