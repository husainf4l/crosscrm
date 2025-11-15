'use client';

import { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import Logo from '@/components/Logo';

interface FormData {
  email: string;
  password: string;
  rememberMe: boolean;
}

interface FormErrors {
  email?: string;
  password?: string;
  general?: string;
}

interface FormState {
  data: FormData;
  errors: FormErrors;
  isSubmitting: boolean;
  isValid: boolean;
}

const initialFormData: FormData = {
  email: '',
  password: '',
  rememberMe: false,
};

const initialFormState: FormState = {
  data: initialFormData,
  errors: {},
  isSubmitting: false,
  isValid: false,
};

export default function LoginPage() {
  const router = useRouter();
  const [formState, setFormState] = useState<FormState>(initialFormState);
  const [showPassword, setShowPassword] = useState(false);

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
    if (password.length < 6) {
      return 'Password must be at least 6 characters';
    }
    return undefined;
  }, []);

  // Update form data and validate
  const updateField = useCallback((field: keyof FormData, value: string | boolean) => {
    setFormState(prev => {
      const newData = { ...prev.data, [field]: value };
      const newErrors = { ...prev.errors };

      // Clear field-specific errors (only for fields that can have errors)
      if (field === 'email' || field === 'password') {
        delete newErrors[field];
      }

      // Clear general error when user starts typing
      delete newErrors.general;

      // Validate the specific field
      if (field === 'email') {
        newErrors.email = validateEmail(value as string);
      } else if (field === 'password') {
        newErrors.password = validatePassword(value as string);
      }

      // Clear general error when user starts typing
      delete newErrors.general;

      // Check if form is valid
      const isValid = !newErrors.email && !newErrors.password &&
                     newData.email.trim() !== '' &&
                     newData.password.length >= 6;

      return {
        ...prev,
        data: newData,
        errors: newErrors,
        isValid,
      };
    });
  }, [validateEmail, validatePassword]);

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Validate all fields before submission
    const emailError = validateEmail(formState.data.email);
    const passwordError = validatePassword(formState.data.password);

    if (emailError || passwordError) {
      setFormState(prev => ({
        ...prev,
        errors: {
          email: emailError,
          password: passwordError,
        },
        isValid: false,
      }));
      return;
    }

    setFormState(prev => ({ ...prev, isSubmitting: true, errors: {} }));

    try {
      // Call GraphQL API for authentication
      const response = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          query: `
            mutation Login($input: LoginDtoInput!) {
              login(input: $input) {
                token
                user {
                  id
                  name
                  email
                  phone
                  createdAt
                  companyId
                }
              }
            }
          `,
          variables: {
            input: {
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

      const { token, user } = result.data.login;

      // Success - store token and user data
      console.log('Login successful:', user);

      localStorage.setItem('authToken', token);
      localStorage.setItem('currentUser', JSON.stringify({
        id: user.id,
        name: user.name,
        email: user.email,
        companyId: user.companyId,
      }));

      // If user doesn't have a company, redirect to company creation
      if (!user.companyId) {
        setTimeout(() => {
          router.push('/create-company');
        }, 1000);
      } else {
        // Otherwise redirect to dashboard
        setTimeout(() => {
          router.push('/dashboard');
        }, 1000);
      }

    } catch (error) {
      setFormState(prev => ({
        ...prev,
        errors: {
          general: error instanceof Error ? error.message : 'Login failed. Please try again.',
        },
        isSubmitting: false,
      }));
    }
  };

  const { data, errors, isSubmitting, isValid } = formState;

  return (
    <div className="min-h-screen bg-white flex items-center justify-center px-4 py-12">
      <div className="relative w-full max-w-md">
        {/* Login Card */}
        <div className="bg-white border border-gray-200 rounded-2xl shadow-lg p-8 sm:p-10">
          {/* Header */}
          <div className="text-center mb-8">
            <div className="mb-6">
              <Logo />
            </div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">Welcome Back</h1>
            <p className="text-gray-600">Sign in to your CrossCRM account</p>
          </div>

          {/* General Error Message */}
          {errors.general && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-sm text-red-600">{errors.general}</p>
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-5" noValidate>
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

            {/* Remember & Forgot */}
            <div className="flex items-center justify-between text-sm">
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={data.rememberMe}
                  onChange={(e) => updateField('rememberMe', e.target.checked)}
                  disabled={isSubmitting}
                  className="w-4 h-4 rounded border-gray-300 bg-white text-blue-600 focus:ring-2 focus:ring-blue-500 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                />
                <span className={`select-none ${isSubmitting ? 'opacity-50' : 'hover:text-gray-900'}`}>
                  Remember me
                </span>
              </label>
              <Link
                href="/forgot-password"
                className="text-blue-600 hover:text-blue-700 transition-colors disabled:opacity-50"
                tabIndex={isSubmitting ? -1 : 0}
              >
                Forgot password?
              </Link>
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
                  <span id="submitting-status">Signing in...</span>
                </>
              ) : (
                <>
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v2a2 2 0 01-2 2H7a2 2 0 01-2-2v-2" />
                  </svg>
                  Sign In
                </>
              )}
            </button>
          </form>

          <p className="text-center text-gray-700 mt-8 text-sm">
            Don&apos;t have an account?{' '}
            <Link
              href="/signup"
              className="text-blue-600 hover:text-blue-700 font-medium transition-colors"
              tabIndex={isSubmitting ? -1 : 0}
            >
              Sign up here
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
