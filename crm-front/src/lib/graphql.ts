import { ApolloClient, InMemoryCache, createHttpLink } from '@apollo/client';
import { setContext } from '@apollo/client/link/context';

const httpLink = createHttpLink({
  uri: 'http://192.168.1.164:5196/graphql',
});

const authLink = setContext((_, { headers }) => {
  // Add authentication token here if needed
  // const token = localStorage.getItem('authToken');
  return {
    headers: {
      ...headers,
      // authorization: token ? `Bearer ${token}` : '',
    }
  };
});

export const client = new ApolloClient({
  link: authLink.concat(httpLink),
  cache: new InMemoryCache(),
});