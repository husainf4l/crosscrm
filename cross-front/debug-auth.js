// Debug script for browser console
// Run this in your browser console to check authentication status

console.log('🔍 Cross CRM Frontend Debug Info:');
console.log('Current URL:', window.location.href);

// Check localStorage tokens
const token = localStorage.getItem('access_token');
const refreshToken = localStorage.getItem('refresh_token');
const user = localStorage.getItem('current_user');

console.log('📊 Authentication Status:');
console.log('Access Token:', token ? 'Present ✅' : 'Missing ❌');
console.log('Refresh Token:', refreshToken ? 'Present ✅' : 'Missing ❌');
console.log('Current User:', user ? 'Present ✅' : 'Missing ❌');

if (token) {
  try {
    const tokenParts = token.split('.');
    const payload = JSON.parse(atob(tokenParts[1]));
    console.log('🔑 Token Payload:', payload);
    console.log('Company ID:', payload.companyId);
    console.log('User ID:', payload.sub);
    console.log('Expires:', new Date(payload.exp * 1000));
  } catch (e) {
    console.error('Failed to decode token:', e);
  }
}

// Set the token manually for testing if needed
function setTestToken() {
  const testToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJhbC1odXNzZWluQHBhcGF5YXRyYWRpbmcuY29tIiwibmFtZSI6ImFsLWh1c3NlaW4iLCJjb21wYW55SWQiOiI2IiwiZXhwIjoxNzYzMzM2Mzg2LCJpc3MiOiJjcm0tYmFja2VuZCIsImF1ZCI6ImNybS1jbGllbnQifQ.rj1dDFXtA7jnXa4cWkeF0LiZSdL6Cpjf1lsCyFn3Ats';
  const testUser = JSON.stringify({
    id: 1,
    email: 'al-hussein@papayatrading.com',
    name: 'al-hussein',
    companyId: 6
  });
  
  localStorage.setItem('access_token', testToken);
  localStorage.setItem('current_user', testUser);
  
  console.log('✅ Test token and user set! Refresh the page.');
}

console.log('💡 To set test authentication, run: setTestToken()');

// Make setTestToken available globally
window.setTestToken = setTestToken;