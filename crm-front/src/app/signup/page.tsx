'use client';

import { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import Logo from '@/components/Logo';

interface FormData {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  agreeToTerms: boolean;
}

interface FormErrors {
  firstName?: string;
  lastName?: string;
  email?: string;
  password?: string;
  confirmPassword?: string;
  agreeToTerms?: string;
  general?: string;
}

interface FormState {
  data: FormData;
  errors: FormErrors;
  isSubmitting: boolean;
  isValid: boolean;
}

const initialFormData: FormData = {
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  confirmPassword: '',
  agreeToTerms: false,
};

const initialFormState: FormState = {
  data: initialFormData,
  errors: {},
  isSubmitting: false,
  isValid: false,
};

export default function SignupPage() {
  const router = useRouter();
  const [formState, setFormState] = useState<FormState>(initialFormState);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  // Name validation
  const validateName = useCallback((name: string, fieldName: string): string | undefined => {
    if (!name.trim()) {
      return `${fieldName} is required`;
    }
    if (name.trim().length < 2) {
      return `${fieldName} must be at least 2 characters`;
    }
    if (!/^[a-zA-Z\s]+$/.test(name.trim())) {
      return `${fieldName} can only contain letters and spaces`;
    }
    return undefined;
  }, []);

  // Email validation
  const validateEmail = useCallback((email: string): string | undefined => {
    if (!email.trim()) {
      return 'Email is required';
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      return 'Please enter a valid email address';
    }
    return undefined;
  }, []);

  // Password validation
  const validatePassword = useCallback((password: string): string | undefined => {
    if (!password) {
      return 'Password is required';
    }
    if (password.length < 8) {
      return 'Password must be at least 8 characters';
    }
    if (!/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/.test(password)) {
      return 'Password must contain at least one uppercase letter, one lowercase letter, and one number';
    }
    return undefined;
  }, []);

  // Confirm password validation
  const validateConfirmPassword = useCallback((confirmPassword: string, password: string): string | undefined => {
    if (!confirmPassword) {
      return 'Please confirm your password';
    }
    if (confirmPassword !== password) {
      return 'Passwords do not match';
    }
    return undefined;
  }, []);

  // Update form data and validate
  const updateField = useCallback((field: keyof FormData, value: string | boolean) => {
    setFormState(prev => {
      const newData = { ...prev.data, [field]: value };
      const newErrors = { ...prev.errors };

      // Clear field-specific errors (only for fields that can have errors)
      if (field !== 'agreeToTerms') {
        delete newErrors[field];
      }

      // Clear general error when user starts typing
      delete newErrors.general;

      // Validate the specific field
      if (field === 'firstName') {
        newErrors.firstName = validateName(value as string, 'First name');
      } else if (field === 'lastName') {
        newErrors.lastName = validateName(value as string, 'Last name');
      } else if (field === 'email') {
        newErrors.email = validateEmail(value as string);
      } else if (field === 'password') {
        newErrors.password = validatePassword(value as string);
        // Re-validate confirm password if password changes
        if (newData.confirmPassword) {
          newErrors.confirmPassword = validateConfirmPassword(newData.confirmPassword, value as string);
        }
      } else if (field === 'confirmPassword') {
        newErrors.confirmPassword = validateConfirmPassword(value as string, newData.password);
      } else if (field === 'agreeToTerms') {
        if (!value) {
          newErrors.agreeToTerms = 'You must agree to the terms and conditions';
        } else {
          delete newErrors.agreeToTerms;
        }
      }

      // Check if form is valid
      const isValid = !newErrors.firstName && !newErrors.lastName && !newErrors.email &&
                     !newErrors.password && !newErrors.confirmPassword && !newErrors.agreeToTerms &&
                     newData.firstName.trim() !== '' && newData.lastName.trim() !== '' &&
                     newData.email.trim() !== '' && newData.password.length >= 8 &&
                     newData.confirmPassword === newData.password && newData.agreeToTerms;

      return {
        ...prev,
        data: newData,
        errors: newErrors,
        isValid,
      };
    });
  }, [validateName, validateEmail, validatePassword, validateConfirmPassword]);

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Validate all fields before submission
    const firstNameError = validateName(formState.data.firstName, 'First name');
    const lastNameError = validateName(formState.data.lastName, 'Last name');
    const emailError = validateEmail(formState.data.email);
    const passwordError = validatePassword(formState.data.password);
    const confirmPasswordError = validateConfirmPassword(formState.data.confirmPassword, formState.data.password);
    const agreeToTermsError = !formState.data.agreeToTerms ? 'You must agree to the terms and conditions' : undefined;

    const allErrors = {
      firstName: firstNameError,
      lastName: lastNameError,
      email: emailError,
      password: passwordError,
      confirmPassword: confirmPasswordError,
      agreeToTerms: agreeToTermsError,
    };

    const hasErrors = Object.values(allErrors).some(error => error !== undefined);

    if (hasErrors) {
      setFormState(prev => ({
        ...prev,
        errors: allErrors,
        isValid: false,
      }));
      return;
    }

    setFormState(prev => ({ ...prev, isSubmitting: true, errors: {} }));

    try {
      // Call GraphQL API to register user
      const response = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          query: `
            mutation Register($input: RegisterDtoInput!) {
              register(input: $input) {
                token
                user {
                  id
                  name
                  email
                  companyId
                }
              }
            }
          `,
          variables: {
            input: {
              name: `${formState.data.firstName} ${formState.data.lastName}`,
              email: formState.data.email,
              password: formState.data.password,
            }
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        throw new Error(result.errors[0].message);
      }

      const { token, user } = result.data.register;

      // Success - log and redirect to company creation
      console.log('Registration successful:', user);

      // Store user data and token in localStorage
      localStorage.setItem('authToken', token);
      localStorage.setItem('currentUser', JSON.stringify({
        id: user.id,
        name: user.name,
        email: user.email,
        companyId: user.companyId,
      }));

      // Redirect to company creation page after 1 second
      setTimeout(() => {
        router.push('/create-company');
      }, 1000);

    } catch (error) {
      setFormState(prev => ({
        ...prev,
        errors: {
          general: error instanceof Error ? error.message : 'Registration failed. Please try again.',
        },
        isSubmitting: false,
      }));
    }
  };

  const { data, errors, isSubmitting, isValid } = formState;

  return (
    <div className="min-h-screen bg-white flex items-center justify-center px-4 py-12">
      <div className="relative w-full max-w-md">
        {/* Signup Card */}
        <div className="bg-white border border-gray-200 rounded-2xl shadow-lg p-8 sm:p-10">
          {/* Header */}
          <div className="text-center mb-8">
            <div className="mb-6">
              <Logo />
            </div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">Create Account</h1>
            <p className="text-gray-600">Join CrossCRM and start managing your business</p>
          </div>

          {/* General Error Message */}
          {errors.general && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-sm text-red-600">{errors.general}</p>
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-5" noValidate>
            {/* Name Fields */}
            <div className="grid grid-cols-2 gap-4">
              {/* First Name */}
              <div>
                <label htmlFor="firstName" className="block text-sm font-medium text-gray-700 mb-2">
                  First Name
                </label>
                <input
                  id="firstName"
                  type="text"
                  value={data.firstName}
                  onChange={(e) => updateField('firstName', e.target.value)}
                  placeholder="John"
                  required
                  disabled={isSubmitting}
                  aria-invalid={!!errors.firstName}
                  aria-describedby={errors.firstName ? 'firstName-error' : undefined}
                  className={`w-full px-4 py-3 bg-white border rounded-lg text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 transition-all duration-200 ${
                    errors.firstName
                      ? 'border-red-300 focus:border-red-500 focus:ring-red-500/50'
                      : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500/50'
                  } ${isSubmitting ? 'opacity-50 cursor-not-allowed' : ''}`}
                />
                {errors.firstName && (
                  <p id="firstName-error" className="mt-1 text-sm text-red-600" role="alert">
                    {errors.firstName}
                  </p>
                )}
              </div>

              {/* Last Name */}
              <div>
                <label htmlFor="lastName" className="block text-sm font-medium text-gray-700 mb-2">
                  Last Name
                </label>
                <input
                  id="lastName"
                  type="text"
                  value={data.lastName}
                  onChange={(e) => updateField('lastName', e.target.value)}
                  placeholder="Doe"
                  required
                  disabled={isSubmitting}
                  aria-invalid={!!errors.lastName}
                  aria-describedby={errors.lastName ? 'lastName-error' : undefined}
                  className={`w-full px-4 py-3 bg-white border rounded-lg text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 transition-all duration-200 ${
                    errors.lastName
                      ? 'border-red-300 focus:border-red-500 focus:ring-red-500/50'
                      : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500/50'
                  } ${isSubmitting ? 'opacity-50 cursor-not-allowed' : ''}`}
                />
                {errors.lastName && (
                  <p id="lastName-error" className="mt-1 text-sm text-red-600" role="alert">
                    {errors.lastName}
                  </p>
                )}
              </div>
            </div>

            {/* Email Input */}
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-2">
                Email Address
              </label>
              <input
                id="email"
                type="email"
                value={data.email}
                onChange={(e) => updateField('email', e.target.value)}
                placeholder="you@example.com"
                required
                disabled={isSubmitting}
                aria-invalid={!!errors.email}
                aria-describedby={errors.email ? 'email-error' : undefined}
                className={`w-full px-4 py-3 bg-white border rounded-lg text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 transition-all duration-200 ${
                  errors.email
                    ? 'border-red-300 focus:border-red-500 focus:ring-red-500/50'
                    : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500/50'
                } ${isSubmitting ? 'opacity-50 cursor-not-allowed' : ''}`}
              />
              {errors.email && (
                <p id="email-error" className="mt-1 text-sm text-red-600" role="alert">
                  {errors.email}
                </p>
              )}
            </div>

            {/* Password Input */}
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-2">
                Password
              </label>
              <div className="relative">
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  value={data.password}
                  onChange={(e) => updateField('password', e.target.value)}
                  placeholder="••••••••"
                  required
                  disabled={isSubmitting}
                  aria-invalid={!!errors.password}
                  aria-describedby={errors.password ? 'password-error' : undefined}
                  className={`w-full px-4 py-3 bg-white border rounded-lg text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 transition-all duration-200 pr-12 ${
                    errors.password
                      ? 'border-red-300 focus:border-red-500 focus:ring-red-500/50'
                      : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500/50'
                  } ${isSubmitting ? 'opacity-50 cursor-not-allowed' : ''}`}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  disabled={isSubmitting}
                  className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                >
                  {showPassword ? (
                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                      <path d="M12 5C7 5 2.73 8.11 1 12.46c1.73 4.35 6 7.54 11 7.54s9.27-3.19 11-7.54C21.27 8.11 17 5 12 5m0 12.5c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5m0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z" />
                    </svg>
                  ) : (
                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                      <path d="M12 7c2.76 0 5 2.24 5 5 0 .65-.13 1.26-.36 1.83l2.92 2.92c1.51-1.26 2.81-2.94 3.69-4.95-2.5-4.38-7.05-7.3-12.25-7.3-1.56 0-3.06.26-4.5.77l2.16 2.16C10.74 7.13 11.35 7 12 7zM2 4.27l2.28 2.28.46.46A11.804 11.804 0 001 11.5c2.5 4.38 7.05 7.3 12.25 7.3 1.66 0 3.26-.25 4.8-.72l.5.5L19.73 22 21 20.73 3.27 3 2 4.27zM7.53 9.8l1.55 1.55c-.05.21-.08.43-.08.65 0 1.66 1.34 3 3 3 .22 0 .44-.03.65-.08l1.55 1.55c-.67.33-1.41.53-2.2.53-2.76 0-5-2.24-5-5 0-.79.2-1.53.53-2.2zm5.31-7.78l3.15 3.15.02-.02c.37-.35.75-.67 1.15-.96l-4.32-2.17zm3.15 11.78l-1.55-1.55c.05-.21.08-.43.08-.65 0-1.66-1.34-3-3-3-.22 0-.44.03-.65.08l-1.55-1.55c.67-.33 1.41-.53 2.2-.53 2.76 0 5 2.24 5 5 0 .79-.2 1.53-.53 2.2z" />
                    </svg>
                  )}
                </button>
              </div>
              {errors.password && (
                <p id="password-error" className="mt-1 text-sm text-red-600" role="alert">
                  {errors.password}
                </p>
              )}
            </div>

            {/* Confirm Password Input */}
            <div>
              <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700 mb-2">
                Confirm Password
              </label>
              <div className="relative">
                <input
                  id="confirmPassword"
                  type={showConfirmPassword ? 'text' : 'password'}
                  value={data.confirmPassword}
                  onChange={(e) => updateField('confirmPassword', e.target.value)}
                  placeholder="••••••••"
                  required
                  disabled={isSubmitting}
                  aria-invalid={!!errors.confirmPassword}
                  aria-describedby={errors.confirmPassword ? 'confirmPassword-error' : undefined}
                  className={`w-full px-4 py-3 bg-white border rounded-lg text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 transition-all duration-200 pr-12 ${
                    errors.confirmPassword
                      ? 'border-red-300 focus:border-red-500 focus:ring-red-500/50'
                      : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500/50'
                  } ${isSubmitting ? 'opacity-50 cursor-not-allowed' : ''}`}
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  disabled={isSubmitting}
                  className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                  aria-label={showConfirmPassword ? 'Hide password' : 'Show password'}
                >
                  {showConfirmPassword ? (
                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                      <path d="M12 5C7 5 2.73 8.11 1 12.46c1.73 4.35 6 7.54 11 7.54s9.27-3.19 11-7.54C21.27 8.11 17 5 12 5m0 12.5c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5m0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z" />
                    </svg>
                  ) : (
                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                      <path d="M12 7c2.76 0 5 2.24 5 5 0 .65-.13 1.26-.36 1.83l2.92 2.92c1.51-1.26 2.81-2.94 3.69-4.95-2.5-4.38-7.05-7.3-12.25-7.3-1.56 0-3.06.26-4.5.77l2.16 2.16C10.74 7.13 11.35 7 12 7zM2 4.27l2.28 2.28.46.46A11.804 11.804 0 001 11.5c2.5 4.38 7.05 7.3 12.25 7.3 1.66 0 3.26-.25 4.8-.72l.5.5L19.73 22 21 20.73 3.27 3 2 4.27zM7.53 9.8l1.55 1.55c-.05.21-.08.43-.08.65 0 1.66 1.34 3 3 3 .22 0 .44-.03.65-.08l1.55 1.55c-.67.33-1.41.53-2.2.53-2.76 0-5-2.24-5-5 0-.79.2-1.53.53-2.2zm5.31-7.78l3.15 3.15.02-.02c.37-.35.75-.67 1.15-.96l-4.32-2.17zm3.15 11.78l-1.55-1.55c.05-.21.08-.43.08-.65 0-1.66-1.34-3-3-3-.22 0-.44.03-.65.08l-1.55-1.55c.67-.33 1.41-.53 2.2-.53 2.76 0 5 2.24 5 5 0 .79-.2 1.53-.53 2.2z" />
                    </svg>
                  )}
                </button>
              </div>
              {errors.confirmPassword && (
                <p id="confirmPassword-error" className="mt-1 text-sm text-red-600" role="alert">
                  {errors.confirmPassword}
                </p>
              )}
            </div>

            {/* Terms and Conditions */}
            <div>
              <label className="flex items-start gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={data.agreeToTerms}
                  onChange={(e) => updateField('agreeToTerms', e.target.checked)}
                  disabled={isSubmitting}
                  required
                  aria-invalid={!!errors.agreeToTerms}
                  aria-describedby={errors.agreeToTerms ? 'terms-error' : undefined}
                  className="mt-1 w-4 h-4 rounded border-gray-300 bg-white text-blue-600 focus:ring-2 focus:ring-blue-500 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                />
                <span className={`text-sm select-none leading-relaxed ${isSubmitting ? 'opacity-50' : 'hover:text-gray-900'}`}>
                  I agree to the{' '}
                  <Link
                    href="/terms"
                    className="text-blue-600 hover:text-blue-700 underline"
                    tabIndex={isSubmitting ? -1 : 0}
                  >
                    Terms of Service
                  </Link>
                  {' '}and{' '}
                  <Link
                    href="/privacy"
                    className="text-blue-600 hover:text-blue-700 underline"
                    tabIndex={isSubmitting ? -1 : 0}
                  >
                    Privacy Policy
                  </Link>
                </span>
              </label>
              {errors.agreeToTerms && (
                <p id="terms-error" className="mt-2 text-sm text-red-600" role="alert">
                  {errors.agreeToTerms}
                </p>
              )}
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
                  <span id="submitting-status">Creating account...</span>
                </>
              ) : (
                <>
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                  </svg>
                  Create Account
                </>
              )}
            </button>
          </form>

          {/* Sign in link */}
          <p className="text-center text-gray-700 mt-8 text-sm">
            Already have an account?{' '}
            <Link
              href="/login"
              className="text-blue-600 hover:text-blue-700 font-medium transition-colors"
              tabIndex={isSubmitting ? -1 : 0}
            >
              Sign in here
            </Link>
          </p>
        </div>

        {/* Bottom decoration */}
        <p className="text-center text-gray-500 text-xs mt-8">
          © 2025 CrossCRM. All rights reserved.
        </p>
      </div>
    </div>
  );
}