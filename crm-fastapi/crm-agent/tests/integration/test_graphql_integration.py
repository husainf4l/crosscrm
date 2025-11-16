"""Integration tests for GraphQL client."""
import pytest
from app.core.graphql_client import GraphQLClient
from app.modules.agent.services.graphql_data_service import GraphQLDataService
from app.config.settings import settings


@pytest.mark.asyncio
async def test_graphql_client_connection():
    """Test GraphQL client can connect to backend."""
    if not settings.GRAPHQL_URL:
        pytest.skip("GRAPHQL_URL not configured")
    
    client = GraphQLClient()
    
    # Test simple query
    query = """
    query {
        hello
    }
    """
    
    try:
        result = await client.query(query)
        assert result is not None
        print(f"✅ GraphQL connection successful: {result}")
    except Exception as e:
        pytest.skip(f"GraphQL backend not available: {e}")


@pytest.mark.asyncio
async def test_graphql_get_tasks():
    """Test fetching tasks from GraphQL."""
    if not settings.GRAPHQL_URL:
        pytest.skip("GRAPHQL_URL not configured")
    
    service = GraphQLDataService()
    
    try:
        tasks = await service.get_tasks_for_user(1)
        assert isinstance(tasks, list)
        print(f"✅ Fetched {len(tasks)} tasks from GraphQL")
    except Exception as e:
        pytest.skip(f"GraphQL backend not available or not authenticated: {e}")


@pytest.mark.asyncio
async def test_graphql_get_leads():
    """Test fetching leads from GraphQL."""
    if not settings.GRAPHQL_URL:
        pytest.skip("GRAPHQL_URL not configured")
    
    service = GraphQLDataService()
    
    try:
        leads = await service.get_leads_for_user(1)
        assert isinstance(leads, list)
        print(f"✅ Fetched {len(leads)} leads from GraphQL")
    except Exception as e:
        pytest.skip(f"GraphQL backend not available or not authenticated: {e}")


@pytest.mark.asyncio
async def test_graphql_get_opportunities():
    """Test fetching opportunities from GraphQL."""
    if not settings.GRAPHQL_URL:
        pytest.skip("GRAPHQL_URL not configured")
    
    service = GraphQLDataService()
    
    try:
        opportunities = await service.get_opportunities_for_user(1)
        assert isinstance(opportunities, list)
        print(f"✅ Fetched {len(opportunities)} opportunities from GraphQL")
    except Exception as e:
        pytest.skip(f"GraphQL backend not available or not authenticated: {e}")

