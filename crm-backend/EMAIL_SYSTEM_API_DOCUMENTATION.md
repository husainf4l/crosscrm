# Email System - GraphQL API Documentation

## Overview

The Cross CRM backend includes a comprehensive email system powered by SendGrid for transactional emails including user invitations, welcome messages, password resets, and notifications.

## Email System GraphQL API

### Send Test Email
Tests the email configuration by sending a test message.

```graphql
mutation SendTestEmail($toEmail: String!) {
  sendTestEmail(toEmail: $toEmail)
}
```

**Authentication**: Required  
**Returns**: `Boolean` (true if email sent successfully)

**Example Request:**
```bash
curl -X POST http://localhost:5196/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "query": "mutation SendTestEmail($toEmail: String!) { sendTestEmail(toEmail: $toEmail) }",
    "variables": { "toEmail": "test@example.com" }
  }'
```

**Example Response:**
```json
{
  "data": {
    "sendTestEmail": true
  }
}
```

## Email Configuration

The email system is configured with the following environment variables:

- `SENDGRID_API_KEY`: SendGrid API key for sending emails
- `FROM_EMAIL`: Default sender email address (e.g., noreply@aqlaan.com)
- `FROM_NAME`: Default sender name (e.g., Cross CRM System)
- `FRONTEND_URL`: Frontend application URL for generating links

## Available Email Templates

### 1. Test Email
Basic email to verify SendGrid configuration.

### 2. Invitation Email
Sent when a user is invited to join a company.
- **Subject**: "You're invited to join {CompanyName} on Cross CRM"
- **Content**: Includes invitation link and expiration information
- **Link Format**: `{FRONTEND_URL}/accept-invitation/{invitationToken}`

### 3. Welcome Email
Sent when a user successfully registers or accepts an invitation.
- **Subject**: "Welcome to {CompanyName} - Cross CRM"
- **Content**: Welcome message and getting started information

### 4. Password Reset Email
Sent when a user requests a password reset.
- **Subject**: "Reset Your Cross CRM Password"
- **Content**: Secure reset link with expiration

### 5. Notification Email
General purpose notification email for system events.
- **Subject**: Customizable based on notification type
- **Content**: Event-specific information

## Integration Examples

### React Component for Email Testing
```typescript
import React, { useState } from 'react';
import { useMutation, gql } from '@apollo/client';

const SEND_TEST_EMAIL = gql`
  mutation SendTestEmail($toEmail: String!) {
    sendTestEmail(toEmail: $toEmail)
  }
`;

export const EmailTester: React.FC = () => {
  const [email, setEmail] = useState('');
  const [sendTestEmail, { loading, error }] = useMutation(SEND_TEST_EMAIL);

  const handleSendTest = async () => {
    try {
      const result = await sendTestEmail({
        variables: { toEmail: email }
      });
      
      if (result.data.sendTestEmail) {
        alert('Test email sent successfully!');
      } else {
        alert('Failed to send test email');
      }
    } catch (err) {
      console.error('Error sending test email:', err);
    }
  };

  return (
    <div>
      <input
        type="email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        placeholder="Enter email address"
      />
      <button 
        onClick={handleSendTest}
        disabled={loading || !email}
      >
        {loading ? 'Sending...' : 'Send Test Email'}
      </button>
      {error && <p>Error: {error.message}</p>}
    </div>
  );
};
```

### Vue.js Composable for Email
```typescript
import { ref } from 'vue';
import { useMutation } from '@vue/apollo-composable';
import { gql } from 'graphql-tag';

export const useEmailTesting = () => {
  const loading = ref(false);
  const error = ref(null);

  const SEND_TEST_EMAIL = gql`
    mutation SendTestEmail($toEmail: String!) {
      sendTestEmail(toEmail: $toEmail)
    }
  `;

  const { mutate: sendTestEmail } = useMutation(SEND_TEST_EMAIL);

  const testEmailDelivery = async (email: string) => {
    loading.value = true;
    error.value = null;

    try {
      const result = await sendTestEmail({ toEmail: email });
      return result.data.sendTestEmail;
    } catch (err) {
      error.value = err.message;
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    testEmailDelivery,
    loading,
    error
  };
};
```

## Email System Status Monitoring

### Check Email System Health
While there's no specific health check endpoint, you can monitor email system status by:

1. **Testing Email Delivery**: Use the `sendTestEmail` mutation regularly
2. **Monitoring SendGrid Dashboard**: Check SendGrid account for delivery statistics
3. **Application Logs**: Monitor application logs for email-related errors

### Common Issues and Troubleshooting

#### Email Not Received
1. Check spam/junk folder
2. Verify SendGrid API key is valid
3. Ensure FROM_EMAIL domain is verified in SendGrid
4. Check SendGrid activity logs

#### GraphQL Errors
```json
{
  "errors": [
    {
      "message": "SENDGRID_API_KEY environment variable is required."
    }
  ]
}
```

**Solution**: Ensure all required environment variables are set.

#### Rate Limiting
SendGrid has rate limits. If you encounter issues:
- Implement exponential backoff in your frontend
- Consider batching invitation emails
- Monitor your SendGrid usage

## Environment Configuration

### Required Environment Variables
```bash
# SendGrid Configuration
SENDGRID_API_KEY=SG.your_sendgrid_api_key_here
FROM_EMAIL=noreply@yourdomain.com
FROM_NAME=Your Company Name
FRONTEND_URL=https://your-frontend-url.com
```

### Email Domain Setup
1. **Domain Authentication**: Configure your domain in SendGrid
2. **DNS Records**: Add required CNAME records to your DNS
3. **From Email**: Use a from address from your verified domain

## Best Practices

### 1. Email Validation
Always validate email addresses before sending:
```typescript
const isValidEmail = (email: string) => {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
};
```

### 2. Error Handling
Implement proper error handling for email failures:
```typescript
try {
  await sendTestEmail({ toEmail: email });
} catch (error) {
  if (error.message.includes('invalid email')) {
    setError('Please enter a valid email address');
  } else {
    setError('Failed to send email. Please try again.');
  }
}
```

### 3. User Feedback
Provide clear feedback to users:
- Show loading states during email sending
- Display success/error messages
- Allow users to resend if needed

### 4. Privacy and Compliance
- Follow GDPR/privacy regulations
- Include unsubscribe links in appropriate emails
- Respect user email preferences

This email system provides reliable transactional email capabilities for your Cross CRM application with proper error handling and monitoring.