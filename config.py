from pydantic_settings import BaseSettings
from typing import Optional


class Settings(BaseSettings):
    # Database
    DATABASE_URL: str = "postgresql://husain:tt55oo77@149.200.251.12:5432/cross"
    
    # Security
    SECRET_KEY: str = "your-secret-key-change-in-production-please"
    ALGORITHM: str = "HS256"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 30
    
    # AI
    OPENAI_API_KEY: Optional[str] = None
    
    # App
    DEBUG: bool = True
    APP_NAME: str = "Cross CRM"
    
    class Config:
        env_file = ".env"
        case_sensitive = True


settings = Settings()

