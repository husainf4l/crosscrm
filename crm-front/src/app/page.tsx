'use client';

import { useState } from 'react';
import Link from 'next/link';

export default function Home() {
  const [isVideoPlaying, setIsVideoPlaying] = useState(false);

  return (
    <div className="min-h-screen bg-white">
      {/* Hero Section */}
      <section className="relative overflow-hidden pt-20 pb-32 sm:pt-32 sm:pb-48 bg-white">

        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 lg:gap-8 items-center">
            {/* Left Content */}
            <div className="flex flex-col justify-center space-y-8">
              {/* Headline */}
              <div>
                <h1 className="text-5xl sm:text-6xl lg:text-7xl font-bold tracking-tight">
                  <span className="text-gray-900">Your Smart CRM That</span>
                  <br />
                  <span className="text-blue-600">
                    Thinks Ahead
                  </span>
                </h1>
              </div>

              {/* Sub-text */}
              <p className="text-xl sm:text-2xl text-gray-600 leading-relaxed max-w-xl">
                AI that understands your customers, predicts actions, automates follow-ups, and boosts sales — all in one clean dashboard.
              </p>

              {/* CTA Buttons */}
              <div className="flex flex-col sm:flex-row gap-4 pt-4">
                <Link
                  href="/signup"
                  className="px-8 py-4 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-xl transition-all duration-300 transform hover:scale-105 shadow-lg hover:shadow-2xl inline-flex items-center justify-center gap-2"
                >
                  <svg
                    className="w-5 h-5"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M13 10V3L4 14h7v7l9-11h-7z"
                    />
                  </svg>
                  Start Free Trial
                </Link>

                <button
                  onClick={() => setIsVideoPlaying(true)}
                  className="px-8 py-4 bg-gray-100 hover:bg-gray-200 text-gray-900 font-semibold rounded-xl transition-all duration-300 transform hover:scale-105 border border-gray-300 inline-flex items-center justify-center gap-2"
                >
                  <svg
                    className="w-5 h-5"
                    fill="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path d="M8 5v14l11-7z" />
                  </svg>
                  Watch Demo
                </button>
              </div>

              {/* Trust badges */}
              <div className="flex items-center gap-6 pt-4">
                <div>
                  <p className="text-sm font-semibold text-gray-900">
                    Join thousands of businesses
                  </p>
                  <p className="text-xs text-gray-600">
                    Start your CRM journey today
                  </p>
                </div>
              </div>
            </div>

            {/* Right Content - 3D Dashboard Mockup */}
            <div className="relative h-96 sm:h-[500px] lg:h-[600px] perspective">
              {/* 3D Dashboard Card with Apple-style lighting */}
              <div className="relative w-full h-full">
                {/* Main dashboard mockup */}
                <div className="relative w-full h-full rounded-2xl overflow-hidden shadow-2xl transform transition-transform duration-500 hover:scale-105 hover:shadow-3xl bg-white border border-gray-200"
                  style={{
                    perspective: '1000px',
                    transformStyle: 'preserve-3d',
                  }}
                >
                  {/* Outer frame */}
                  <div className="absolute inset-0 bg-white rounded-2xl p-1">
                    {/* Inner content */}
                    <div className="w-full h-full bg-white rounded-xl border border-gray-100 overflow-hidden">
                      {/* Dashboard content preview */}
                      <div className="w-full h-full flex flex-col">
                        {/* Top bar */}
                        <div className="h-12 bg-gray-50 border-b border-gray-200 flex items-center px-6">
                          <div className="flex gap-1">
                            <div className="w-3 h-3 rounded-full bg-red-500"></div>
                            <div className="w-3 h-3 rounded-full bg-yellow-500"></div>
                            <div className="w-3 h-3 rounded-full bg-green-500"></div>
                          </div>
                          <div className="ml-auto text-xs text-gray-500">
                            dashboard.crosscrm.io
                          </div>
                        </div>

                        {/* Content area */}
                        <div className="flex-1 p-6 space-y-4">
                          {/* Header */}
                          <div className="space-y-2">
                            <div className="h-6 bg-gray-200 rounded-lg w-2/3"></div>
                            <div className="h-4 bg-gray-100 rounded w-1/2"></div>
                          </div>

                          {/* Stats grid */}
                          <div className="grid grid-cols-2 gap-4 pt-2">
                            {[1, 2, 3, 4].map((i) => (
                              <div
                                key={i}
                                className="bg-gray-50 rounded-lg p-3 border border-gray-200"
                              >
                                <div className="h-3 bg-gray-200 rounded w-3/4 mb-2"></div>
                                <div className="h-4 bg-gray-100 rounded"></div>
                              </div>
                            ))}
                          </div>

                          {/* Chart placeholder */}
                          <div className="mt-4 space-y-2">
                            <div className="h-2 bg-gray-300 rounded-full"></div>
                            <div className="h-2 bg-gray-200 rounded-full w-3/4"></div>
                            <div className="h-2 bg-gray-100 rounded-full w-1/2"></div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Shadow under the mockup */}
                  <div className="absolute -bottom-8 left-1/2 transform -translate-x-1/2 w-3/4 h-12 bg-gray-300/20 rounded-full blur-xl"></div>
                </div>

                {/* Floating badge */}
                <div className="absolute bottom-8 right-0 bg-white rounded-full px-4 py-2 shadow-lg border border-gray-200">
                  <p className="text-xs font-semibold text-gray-900">
                    Dashboard Preview
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Video Modal */}
      {isVideoPlaying && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4"
          onClick={() => setIsVideoPlaying(false)}
        >
          <div className="relative w-full max-w-4xl">
            <button
              onClick={() => setIsVideoPlaying(false)}
              className="absolute -top-12 right-0 text-white hover:text-gray-300 transition-colors"
            >
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>

            {/* Video Container */}
            <div className="relative w-full pt-[56.25%] bg-black rounded-xl overflow-hidden shadow-2xl">
              <div className="absolute inset-0 flex items-center justify-center bg-black">
                <div className="text-center">
                  <svg className="w-24 h-24 mx-auto text-blue-500 mb-4" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M8 5v14l11-7z" />
                  </svg>
                  <p className="text-white text-lg font-semibold">Demo Video Coming Soon</p>
                  <p className="text-gray-400 text-sm mt-2">High-quality demo will be added soon</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Smart Highlights Bar */}
      <section className="relative py-16 sm:py-20 border-t border-gray-200 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Section Label */}
          <div className="text-center mb-12">
            <p className="text-sm font-semibold text-blue-600 uppercase tracking-widest mb-2">
              Core Features
            </p>
            <h2 className="text-3xl sm:text-4xl font-bold text-gray-900 mb-2">
              Everything You Need to Win
            </h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Powerful features designed for modern sales teams
            </p>
          </div>

          {/* Highlights Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-6 lg:gap-4">
            {/* Feature 1 - AI Auto-Follow-ups */}
            <div className="group relative">
              <div className="h-full bg-white rounded-xl p-6 border border-gray-200 hover:border-blue-400 transition-all duration-300 hover:shadow-lg">
                {/* Icon */}
                <div className="mb-4">
                  <div className="w-12 h-12 rounded-lg bg-blue-50 flex items-center justify-center group-hover:scale-110 transition-transform duration-300">
                    <svg
                      className="w-6 h-6 text-blue-600 stroke-2"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M13 10V3L4 14h7v7l9-11h-7z"
                      />
                    </svg>
                  </div>
                </div>

                {/* Content */}
                <h3 className="font-semibold text-gray-900 mb-2">
                  AI Auto-Follow-ups
                </h3>
                <p className="text-sm text-gray-600">
                  Never miss a lead with intelligent automated follow-ups
                </p>
              </div>
            </div>

            {/* Feature 2 - Predictive Sales Insights */}
            <div className="group relative">
              <div className="h-full bg-white rounded-xl p-6 border border-gray-200 hover:border-purple-400 transition-all duration-300 hover:shadow-lg">
                {/* Icon */}
                <div className="mb-4">
                  <div className="w-12 h-12 rounded-lg bg-purple-50 flex items-center justify-center group-hover:scale-110 transition-transform duration-300">
                    <svg
                      className="w-6 h-6 text-purple-600 stroke-2"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                      />
                    </svg>
                  </div>
                </div>

                {/* Content */}
                <h3 className="font-semibold text-gray-900 mb-2">
                  Predictive Insights
                </h3>
                <p className="text-sm text-gray-600">
                  AI predicts customer actions and sales opportunities
                </p>
              </div>
            </div>

            {/* Feature 3 - Smart Communication Hub */}
            <div className="group relative">
              <div className="h-full bg-white rounded-xl p-6 border border-gray-200 hover:border-emerald-400 transition-all duration-300 hover:shadow-lg">
                {/* Icon */}
                <div className="mb-4">
                  <div className="w-12 h-12 rounded-lg bg-emerald-50 flex items-center justify-center group-hover:scale-110 transition-transform duration-300">
                    <svg
                      className="w-6 h-6 text-emerald-600 stroke-2"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
                      />
                    </svg>
                  </div>
                </div>

                {/* Content */}
                <h3 className="font-semibold text-gray-900 mb-2">
                  Communication Hub
                </h3>
                <p className="text-sm text-gray-600">
                  All conversations in one unified platform
                </p>
              </div>
            </div>

            {/* Feature 4 - Customer 360° View */}
            <div className="group relative">
              <div className="h-full bg-white rounded-xl p-6 border border-gray-200 hover:border-orange-400 transition-all duration-300 hover:shadow-lg">
                {/* Icon */}
                <div className="mb-4">
                  <div className="w-12 h-12 rounded-lg bg-orange-50 flex items-center justify-center group-hover:scale-110 transition-transform duration-300">
                    <svg
                      className="w-6 h-6 text-orange-600 stroke-2"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                      />
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                      />
                    </svg>
                  </div>
                </div>

                {/* Content */}
                <h3 className="font-semibold text-gray-900 mb-2">
                  360° View
                </h3>
                <p className="text-sm text-gray-600">
                  Complete customer profile at a glance
                </p>
              </div>
            </div>

            {/* Feature 5 - Instant Reports */}
            <div className="group relative">
              <div className="h-full bg-white rounded-xl p-6 border border-gray-200 hover:border-pink-400 transition-all duration-300 hover:shadow-lg">
                {/* Icon */}
                <div className="mb-4">
                  <div className="w-12 h-12 rounded-lg bg-pink-50 flex items-center justify-center group-hover:scale-110 transition-transform duration-300">
                    <svg
                      className="w-6 h-6 text-pink-600 stroke-2"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
                      />
                    </svg>
                  </div>
                </div>

                {/* Content */}
                <h3 className="font-semibold text-gray-900 mb-2">
                  Instant Reports
                </h3>
                <p className="text-sm text-gray-600">
                  Real-time analytics and insights at your fingertips
                </p>
              </div>
            </div>
          </div>

          {/* Bottom accent line */}
          <div className="mt-12 h-px bg-gray-200"></div>
        </div>
      </section>

      {/* Add custom styles for animations */}
      <style>{`
        @keyframes fadeIn {
          from {
            opacity: 0;
          }
          to {
            opacity: 1;
          }
        }
        .animate-fadeIn {
          animation: fadeIn 1.5s ease-in-out;
        }
      `}</style>
    </div>
  );
}
