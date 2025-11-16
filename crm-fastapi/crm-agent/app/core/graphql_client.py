"""GraphQL client for connecting to crm-backend."""
import httpx
from typing import Optional, Dict, Any
from app.config.settings import settings


class GraphQLClient:
    """Client for making GraphQL requests to crm-backend."""
    
    def __init__(self):
        self.url = settings.GRAPHQL_URL
        self.api_key = settings.GRAPHQL_API_KEY
        self.headers = {
            "Content-Type": "application/json",
        }
        
        if self.api_key:
            self.headers["Authorization"] = f"Bearer {self.api_key}"
    
    async def execute(
        self,
        query: str,
        variables: Optional[Dict[str, Any]] = None,
        operation_name: Optional[str] = None
    ) -> Dict[str, Any]:
        """
        Execute a GraphQL query or mutation.
        
        Args:
            query: GraphQL query/mutation string
            variables: Variables for the query
            operation_name: Name of the operation (for multi-operation queries)
            
        Returns:
            GraphQL response dictionary
        """
        payload = {
            "query": query,
        }
        
        if variables:
            payload["variables"] = variables
        
        if operation_name:
            payload["operationName"] = operation_name
        
        async with httpx.AsyncClient(timeout=30.0) as client:
            try:
                response = await client.post(
                    self.url,
                    headers=self.headers,
                    json=payload
                )
                response.raise_for_status()
                result = response.json()
                
                if "errors" in result:
                    raise Exception(f"GraphQL errors: {result['errors']}")
                
                return result.get("data", {})
            except httpx.HTTPError as e:
                raise Exception(f"GraphQL request failed: {str(e)}")
    
    async def query(self, query: str, variables: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
        """Execute a GraphQL query."""
        return await self.execute(query, variables)
    
    async def mutate(self, mutation: str, variables: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
        """Execute a GraphQL mutation."""
        return await self.execute(mutation, variables)

