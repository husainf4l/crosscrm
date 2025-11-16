"""Service to fetch data from crm-backend GraphQL API."""
from typing import List, Optional, Dict, Any
from datetime import date
from app.core.graphql_client import GraphQLClient
from app.config.settings import settings


class GraphQLDataService:
    """Service to fetch CRM data from GraphQL backend."""
    
    def __init__(self):
        self.client = GraphQLClient() if settings.GRAPHQL_URL else None
    
    async def get_tasks_for_user(self, user_id: int, company_id: Optional[int] = None) -> List[Dict[str, Any]]:
        """Get tasks for a user from GraphQL backend."""
        if not self.client:
            return []
        
        query = """
        query GetMyTasks {
            getMyTasks {
                id
                title
                status
                dueDate
                assignedToUserId
            }
        }
        """
        
        try:
            data = await self.client.query(query)
            tasks = data.get("getMyTasks", [])
            
            # Filter by user_id if provided
            if user_id:
                tasks = [t for t in tasks if t.get("assignedToUserId") == user_id]
            
            # Filter by today's date
            today = date.today().isoformat()
            tasks = [t for t in tasks if t.get("dueDate") == today]
            
            return tasks
        except Exception as e:
            print(f"Error fetching tasks from GraphQL: {e}")
            return []
    
    async def get_leads_for_user(self, user_id: int, company_id: Optional[int] = None) -> List[Dict[str, Any]]:
        """Get leads for a user from GraphQL backend."""
        if not self.client:
            return []
        
        query = """
        query GetLeads {
            getLeads {
                id
                firstName
                lastName
                email
                phone
                status
                companyId
                assignedUserId
            }
        }
        """
        
        try:
            data = await self.client.query(query)
            leads = data.get("getLeads", [])
            
            # Filter by user_id if provided
            if user_id:
                leads = [l for l in leads if l.get("assignedUserId") == user_id]
            
            return leads
        except Exception as e:
            print(f"Error fetching leads from GraphQL: {e}")
            return []
    
    async def get_opportunities_for_user(self, user_id: int, company_id: Optional[int] = None) -> List[Dict[str, Any]]:
        """Get opportunities (sales) for a user from GraphQL backend."""
        if not self.client:
            return []
        
        query = """
        query GetOpportunities {
            getOpportunities {
                id
                name
                amount
                stage
                status
                assignedUserId
                customerId
                companyId
            }
        }
        """
        
        try:
            data = await self.client.query(query)
            opportunities = data.get("getOpportunities", [])
            
            # Filter by user_id if provided
            if user_id:
                opportunities = [o for o in opportunities if o.get("assignedUserId") == user_id]
            
            return opportunities
        except Exception as e:
            print(f"Error fetching opportunities from GraphQL: {e}")
            return []
    
    async def get_customers_for_user(self, user_id: int, company_id: Optional[int] = None) -> List[Dict[str, Any]]:
        """Get customers for a user from GraphQL backend."""
        if not self.client:
            return []
        
        query = """
        query GetCustomers($first: Int) {
            getCustomers(first: $first) {
                edges {
                    node {
                        id
                        name
                        email
                        phone
                        status
                        companyId
                    }
                }
            }
        }
        """
        
        try:
            variables = {"first": 100}
            data = await self.client.query(query, variables)
            edges = data.get("getCustomers", {}).get("edges", [])
            customers = [edge.get("node") for edge in edges if edge.get("node")]
            
            return customers
        except Exception as e:
            print(f"Error fetching customers from GraphQL: {e}")
            return []

