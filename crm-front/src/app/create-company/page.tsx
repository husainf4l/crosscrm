'use client';

import { useState, useCallback, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import Logo from '@/components/Logo';

interface FormData {
  companyName: string;
  description: string;
}

interface FormErrors {
  companyName?: string;
  description?: string;
  general?: string;
}

interface FormState {
  data: FormData;
  errors: FormErrors;
  isSubmitting: boolean;
  isValid: boolean;
}

const initialFormData: FormData = {
  companyName: '',
  description: '',
};

const initialFormState: FormState = {
  data: initialFormData,
  errors: {},
  isSubmitting: false,
  isValid: false,
};

export default function CreateCompanyPage() {
  const router = useRouter();
  const [formState, setFormState] = useState<FormState>(initialFormState);
  const [userId, setUserId] = useState<number | null>(null);

  // Get user ID from localStorage on mount and check for existing company
  useEffect(() => {
    const userData = localStorage.getItem('currentUser');
    const token = localStorage.getItem('authToken');

    if (!userData || !token) {
      router.push('/signup');
      return;
    }

    try {
      const user = JSON.parse(userData);
      
      // If user already has a company, redirect to dashboard
      if (user.companyId) {
        router.push('/dashboard');
        return;
      }
      
      setUserId(user.id);
    } catch (error) {
      console.error('Error parsing user data:', error);
      router.push('/signup');
    }
  }, [router]);

  // Company name validation
  const validateCompanyName = useCallback((name: string): string | undefined => {
    if (!name.trim()) {
      return 'Company name is required';
    }
    if (name.trim().length < 2) {
      return 'Company name must be at least 2 characters';
    }
    if (name.trim().length > 100) {
      return 'Company name must be less than 100 characters';
    }
    return undefined;
  }, []);

  // Description validation
  const validateDescription = useCallback((desc: string): string | undefined => {
    if (desc.trim().length > 500) {
      return 'Description must be less than 500 characters';
    }
    return undefined;
  }, []);

  // Update form data and validate
  const updateField = useCallback((field: keyof FormData, value: string) => {
    setFormState(prev => {
      const newData = { ...prev.data, [field]: value };
      const newErrors = { ...prev.errors };

      // Clear field-specific errors
      delete newErrors[field];

      // Clear general error when user starts typing
      delete newErrors.general;

      // Validate the specific field
      if (field === 'companyName') {
        newErrors.companyName = validateCompanyName(value);
      } else if (field === 'description') {
        newErrors.description = validateDescription(value);
      }

      // Check if form is valid
      const isValid = !newErrors.companyName && !newErrors.description &&
                     newData.companyName.trim() !== '';

      return {
        ...prev,
        data: newData,
        errors: newErrors,
        isValid,
      };
    });
  }, [validateCompanyName, validateDescription]);

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (!userId) {
      setFormState(prev => ({
        ...prev,
        errors: { general: 'User not found. Please sign up again.' },
      }));
      return;
    }

    // Validate all fields
    const companyNameError = validateCompanyName(formState.data.companyName);
    const descriptionError = validateDescription(formState.data.description);

    if (companyNameError || descriptionError) {
      setFormState(prev => ({
        ...prev,
        errors: {
          companyName: companyNameError,
          description: descriptionError,
        },
        isValid: false,
      }));
      return;
    }

    setFormState(prev => ({ ...prev, isSubmitting: true, errors: {} }));

    try {
      // Step 1: Create company
      const createCompanyResponse = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          query: `
            mutation CreateCompany($input: CreateCompanyDtoInput!) {
              createCompany(input: $input) {
                id
                name
                description
                createdAt
                userCount
              }
            }
          `,
          variables: {
            input: {
              name: formState.data.companyName,
              description: formState.data.description || null,
            }
          }
        })
      });

      const createCompanyResult = await createCompanyResponse.json();

      if (createCompanyResult.errors) {
        throw new Error(createCompanyResult.errors[0].message);
      }

      const company = createCompanyResult.data.createCompany;

      // Step 2: Add user to company
      const addUserResponse = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          query: `
            mutation AddUserToCompany($userId: Int!, $companyId: Int!) {
              addUserToCompany(userId: $userId, companyId: $companyId)
            }
          `,
          variables: {
            userId,
            companyId: company.id,
          }
        })
      });

      const addUserResult = await addUserResponse.json();

      if (addUserResult.errors) {
        throw new Error(addUserResult.errors[0].message);
      }

      // Step 3: Set the company as active for the user
      const setActiveCompanyResponse = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          query: `
            mutation SetActiveCompany($userId: Int!, $companyId: Int!) {
              setActiveCompany(userId: $userId, companyId: $companyId)
            }
          `,
          variables: {
            userId,
            companyId: company.id,
          }
        })
      });

      const setActiveCompanyResult = await setActiveCompanyResponse.json();

      if (setActiveCompanyResult.errors) {
        throw new Error(setActiveCompanyResult.errors[0].message);
      }

      // Success - update user with companyId
      localStorage.setItem('currentUser', JSON.stringify({
        id: userId,
        name: JSON.parse(localStorage.getItem('currentUser') || '{}').name,
        email: JSON.parse(localStorage.getItem('currentUser') || '{}').email,
        companyId: company.id,
      }));

      console.log('Company created successfully:', company);
      console.log('User added to company successfully');
      console.log('Company activated for user successfully');

      // Redirect to dashboard after 1 second
      setTimeout(() => {
        router.push('/dashboard');
      }, 1000);

    } catch (error) {
      console.error('Company creation error:', error);
      setFormState(prev => ({
        ...prev,
        errors: {
          general: error instanceof Error ? error.message : 'Failed to create company. Please try again.',
        },
        isSubmitting: false,
      }));
    }
  };

  const { data, errors, isSubmitting, isValid } = formState;

  return (
    <div className="min-h-screen bg-white flex items-center justify-center px-4 py-12">
      <div className="relative w-full max-w-md">
        {/* Create Company Card */}
        <div className="bg-white border border-gray-200 rounded-2xl shadow-lg p-8 sm:p-10">
          {/* Header */}
          <div className="text-center mb-8">
            <div className="mb-6">
              <Logo />
            </div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">Create Company</h1>
            <p className="text-gray-600">Set up your company to get started with CrossCRM</p>
          </div>

          {/* General Error Message */}
          {errors.general && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-sm text-red-600">{errors.general}</p>
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-5" noValidate>
            {/* Company Name Input */}
            <div>
              <label htmlFor="companyName" className="block text-sm font-medium text-gray-700 mb-2">
                Company Name
              </label>
              <input
                id="companyName"
                type="text"
                value={data.companyName}
                onChange={(e) => updateField('companyName', e.target.value)}
                placeholder="Your Company Name"
                required
                disabled={isSubmitting}
                aria-invalid={!!errors.companyName}
                aria-describedby={errors.companyName ? 'companyName-error' : undefined}
                className={`w-full px-4 py-3 bg-white border rounded-lg text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 transition-all duration-200 ${
                  errors.companyName
                    ? 'border-red-300 focus:border-red-500 focus:ring-red-500/50'
                    : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500/50'
                } ${isSubmitting ? 'opacity-50 cursor-not-allowed' : ''}`}
              />
              {errors.companyName && (
                <p id="companyName-error" className="mt-1 text-sm text-red-600" role="alert">
                  {errors.companyName}
                </p>
              )}
            </div>

            {/* Description Input */}
            <div>
              <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-2">
                Description <span className="text-gray-400">(Optional)</span>
              </label>
              <textarea
                id="description"
                value={data.description}
                onChange={(e) => updateField('description', e.target.value)}
                placeholder="Tell us about your company..."
                disabled={isSubmitting}
                aria-invalid={!!errors.description}
                aria-describedby={errors.description ? 'description-error' : undefined}
                rows={4}
                className={`w-full px-4 py-3 bg-white border rounded-lg text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 transition-all duration-200 resize-none ${
                  errors.description
                    ? 'border-red-300 focus:border-red-500 focus:ring-red-500/50'
                    : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500/50'
                } ${isSubmitting ? 'opacity-50 cursor-not-allowed' : ''}`}
              />
              {errors.description && (
                <p id="description-error" className="mt-1 text-sm text-red-600" role="alert">
                  {errors.description}
                </p>
              )}
              <p className="mt-1 text-xs text-gray-400">{data.description.length}/500 characters</p>
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isSubmitting || !isValid}
              className={`w-full py-3 mt-8 font-semibold rounded-lg transition-all duration-200 transform flex items-center justify-center gap-2 ${
                isValid && !isSubmitting
                  ? 'bg-blue-600 hover:bg-blue-700 hover:scale-105 shadow-lg hover:shadow-xl text-white'
                  : 'bg-gray-300 text-gray-500 cursor-not-allowed'
              }`}
              aria-describedby={isSubmitting ? 'submitting-status' : undefined}
            >
              {isSubmitting ? (
                <>
                  <svg className="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24" aria-hidden="true">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  <span id="submitting-status">Creating company...</span>
                </>
              ) : (
                <>
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                  </svg>
                  Create Company
                </>
              )}
            </button>
          </form>

          {/* Info Box */}
          <div className="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
            <p className="text-sm text-blue-700">
              💡 <strong>Tip:</strong> You can add more team members and manage your company details later from the dashboard.
            </p>
          </div>
        </div>

        {/* Bottom decoration */}
        <p className="text-center text-gray-500 text-xs mt-8">
          © 2025 CrossCRM. All rights reserved.
        </p>
      </div>
    </div>
  );
}