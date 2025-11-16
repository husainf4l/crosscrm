#!/usr/bin/env python3
"""
Quick script to check if OpenAI API key is configured correctly.
"""
import os
import sys
from pathlib import Path

# Add project root to path
project_root = Path(__file__).parent
sys.path.insert(0, str(project_root))

try:
    from app.config.settings import settings
    from openai import AsyncOpenAI
    import asyncio
except ImportError as e:
    print(f"❌ Error importing modules: {e}")
    print("Make sure you're in the virtual environment:")
    print("  source venv/bin/activate")
    sys.exit(1)


async def test_openai_connection():
    """Test if OpenAI API key works."""
    if not settings.OPENAI_API_KEY or settings.OPENAI_API_KEY == "your_openai_api_key_here":
        print("❌ OPENAI_API_KEY is NOT SET")
        print("\nTo set it up:")
        print("1. Get your API key from: https://platform.openai.com/api-keys")
        print("2. Add it to .env file: OPENAI_API_KEY=sk-your-key-here")
        print("3. Restart the server")
        return False
    
    print(f"✅ OPENAI_API_KEY is SET")
    print(f"   Key starts with: {settings.OPENAI_API_KEY[:10]}...")
    print(f"   Model: {settings.OPENAI_MODEL}")
    print()
    
    # Test the connection
    print("Testing OpenAI API connection...")
    try:
        client = AsyncOpenAI(api_key=settings.OPENAI_API_KEY)
        response = await client.chat.completions.create(
            model=settings.OPENAI_MODEL,
            messages=[
                {"role": "system", "content": "You are a helpful assistant."},
                {"role": "user", "content": "Say 'Hello' if you can hear me."}
            ],
            max_tokens=10
        )
        
        message = response.choices[0].message.content.strip()
        print(f"✅ API Connection Successful!")
        print(f"   Response: {message}")
        print()
        print("🎉 Everything is configured correctly!")
        print("   You can now use the chat interface with LLM responses.")
        return True
        
    except Exception as e:
        error_msg = str(e)
        print(f"❌ API Connection Failed!")
        print(f"   Error: {error_msg}")
        print()
        
        if "api_key" in error_msg.lower() or "authentication" in error_msg.lower():
            print("   This usually means:")
            print("   - The API key is invalid")
            print("   - The API key has been revoked")
            print("   - Check your OpenAI account for issues")
        elif "rate limit" in error_msg.lower():
            print("   Rate limit exceeded. Try again later.")
        elif "insufficient_quota" in error_msg.lower():
            print("   You've run out of credits. Add credits to your OpenAI account.")
        else:
            print("   Check your internet connection and try again.")
        
        return False


if __name__ == "__main__":
    print("=" * 60)
    print("OpenAI API Configuration Checker")
    print("=" * 60)
    print()
    
    result = asyncio.run(test_openai_connection())
    
    print()
    print("=" * 60)
    sys.exit(0 if result else 1)

