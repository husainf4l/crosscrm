# 🔑 Setting Up OpenAI API Key

## Current Status
❌ **OPENAI_API_KEY is NOT SET** (using placeholder: `your_openai_api_key_here`)

## Quick Setup Guide

### Step 1: Get Your OpenAI API Key

1. Go to [OpenAI Platform](https://platform.openai.com/api-keys)
2. Sign in or create an account
3. Click "Create new secret key"
4. Copy the key (it starts with `sk-`)

### Step 2: Update `.env` File

Open the `.env` file in the project root and update this line:

```env
OPENAI_API_KEY=sk-your-actual-api-key-here
```

Replace `sk-your-actual-api-key-here` with your actual API key.

**Example:**
```env
OPENAI_API_KEY=sk-proj-abc123xyz789...
```

### Step 3: Verify the Setup

Run this command to check if the API key is configured:

```bash
cd /home/husain/crosscrm/crm-fastapi/crm-agent
source venv/bin/activate
python -c "from app.config.settings import settings; print('✅ API Key Set!' if settings.OPENAI_API_KEY and settings.OPENAI_API_KEY != 'your_openai_api_key_here' else '❌ API Key Not Set')"
```

### Step 4: Restart the Server

The server should auto-reload, but if it doesn't:

```bash
# Stop the current server (Ctrl+C)
# Then restart:
cd /home/husain/crosscrm/crm-fastapi/crm-agent
source venv/bin/activate
uvicorn app.main:app --reload --host 0.0.0.0 --port 8001
```

### Step 5: Test the Chat

1. Open the chat interface: `http://localhost:8001/chat`
2. Send a message like "Hello" or "What are my tasks?"
3. You should now get natural, dynamic LLM responses!

## Troubleshooting

### If you get "API key is not configured" error:
- Make sure the `.env` file is in the project root (`crm-agent/.env`)
- Check that the key starts with `sk-`
- Restart the server after updating `.env`

### If you get authentication errors:
- Verify your API key is correct
- Check if you have credits in your OpenAI account
- Make sure the key hasn't been revoked

### If responses are still generic:
- The server might need a restart to pick up the new API key
- Check the server logs for any errors

## Security Notes

⚠️ **Important:**
- Never commit your `.env` file to git
- Never share your API key publicly
- The `.env` file is already in `.gitignore` for security

## Current Configuration

- **Model**: `gpt-4o-mini` (cost-effective, fast)
- **Temperature**: `0.8` (dynamic, varied responses)
- **Max Tokens**: `500` (concise responses)

You can change these in `.env` if needed:
```env
OPENAI_MODEL=gpt-4o-mini
```

## Need Help?

If you encounter any issues:
1. Check the server logs for error messages
2. Verify your API key is valid
3. Make sure you have credits in your OpenAI account

