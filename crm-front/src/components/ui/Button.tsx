'use client';

import { ButtonHTMLAttributes, ReactNode } from 'react';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'tertiary' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  isLoading?: boolean;
  children: ReactNode;
}

export default function Button({
  variant = 'primary',
  size = 'md',
  isLoading = false,
  disabled,
  children,
  className = '',
  ...props
}: ButtonProps) {
  const baseStyles = 'inline-flex items-center justify-center font-medium rounded-lg transition-all focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed active:scale-[0.98]';
  
  const variants = {
    primary: {
      base: 'text-white',
      background: 'var(--blue)',
      hover: 'hover:opacity-90',
      focus: 'focus:ring-blue-500',
      shadow: 'var(--shadow-sm)',
    },
    secondary: {
      base: 'text-gray-700',
      background: 'transparent',
      border: '1px solid var(--border)',
      hover: 'hover:bg-gray-50',
      focus: 'focus:ring-gray-500',
    },
    tertiary: {
      base: 'text-gray-700',
      background: 'transparent',
      hover: 'hover:bg-gray-100',
      focus: 'focus:ring-gray-500',
    },
    danger: {
      base: 'text-white',
      background: 'var(--red)',
      hover: 'hover:opacity-90',
      focus: 'focus:ring-red-500',
      shadow: 'var(--shadow-sm)',
    },
  };

  const sizes = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2 text-sm',
    lg: 'px-6 py-3 text-base',
  };

  const variantStyles = variants[variant];
  const sizeStyles = sizes[size];

  const style: React.CSSProperties = {
    backgroundColor: variantStyles.background,
    border: variantStyles.border,
    boxShadow: variantStyles.shadow,
    color: variantStyles.base.includes('text-white') ? 'white' : undefined,
  };

  return (
    <button
      className={`${baseStyles} ${sizeStyles} ${variantStyles.hover} ${variantStyles.focus} ripple ${className}`}
      style={style}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading ? (
        <>
          <svg
            className="animate-spin -ml-1 mr-2 h-4 w-4"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            />
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            />
          </svg>
          Loading...
        </>
      ) : (
        children
      )}
    </button>
  );
}

