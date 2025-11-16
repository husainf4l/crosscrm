import os
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()


class Settings:
    """Application settings loaded from environment variables."""
    
    # Database
    DATABASE_URL: str = os.getenv("DATABASE_URL", "postgresql+asyncpg://user:pass@localhost:5432/crm")
    
    # OpenAI API
    OPENAI_API_KEY: str = os.getenv("OPENAI_API_KEY", "")
    OPENAI_MODEL: str = os.getenv("OPENAI_MODEL", "gpt-4o-mini")
    
    # GraphQL Backend (crm-backend)
    GRAPHQL_URL: str = os.getenv("GRAPHQL_URL", "http://localhost:5000/graphql")
    GRAPHQL_API_KEY: str = os.getenv("GRAPHQL_API_KEY", "")
    
    # JWT Authentication (if needed for GraphQL)
    JWT_SECRET: str = os.getenv("JWT_SECRET", "")
    JWT_ALGORITHM: str = os.getenv("JWT_ALGORITHM", "HS256")
    
    # Application
    DEBUG: bool = os.getenv("DEBUG", "False").lower() == "true"
    API_VERSION: str = os.getenv("API_VERSION", "1.0.0")


settings = Settings()

