"""Sales Assistant AI Agent"""
from typing import Optional, List
from db.models.contact import Contact
from db.models.activity import Activity


def suggest_conversation_starters(contact: Contact) -> List[str]:
    """Generate conversation starters for a contact"""
    suggestions = []
    
    if contact.company:
        suggestions.append(f"Ask about {contact.company.name}'s current business priorities")
        suggestions.append(f"Discuss how {contact.company.name} is handling market challenges")
    
    if contact.job_title:
        suggestions.append(f"Ask about their role as {contact.job_title}")
    
    suggestions.append("Ask about their biggest challenges this quarter")
    suggestions.append("Discuss industry trends and how they're adapting")
    
    return suggestions


def analyze_email_sentiment(email_text: str) -> dict:
    """Analyze email sentiment (basic implementation)"""
    # This is a placeholder - in production, use NLP library or AI service
    positive_words = ["great", "excellent", "happy", "pleased", "thank", "appreciate"]
    negative_words = ["disappointed", "concerned", "issue", "problem", "unhappy"]
    
    text_lower = email_text.lower()
    positive_count = sum(1 for word in positive_words if word in text_lower)
    negative_count = sum(1 for word in negative_words if word in text_lower)
    
    if positive_count > negative_count:
        sentiment = "positive"
    elif negative_count > positive_count:
        sentiment = "negative"
    else:
        sentiment = "neutral"
    
    return {
        "sentiment": sentiment,
        "positive_score": positive_count,
        "negative_score": negative_count
    }


def suggest_follow_up(activities: List[Activity]) -> Optional[str]:
    """Suggest next follow-up action based on activity history"""
    if not activities:
        return "Schedule an initial discovery call"
    
    last_activity = activities[0]  # Assuming sorted by date desc
    
    if last_activity.type.value == "call":
        return "Send a follow-up email summarizing the call and next steps"
    elif last_activity.type.value == "email":
        return "Schedule a follow-up call to discuss the email"
    elif last_activity.type.value == "meeting":
        return "Send a thank you email and proposal if discussed"
    
    return "Schedule a check-in call"

