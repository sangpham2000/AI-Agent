import { UserManager, WebStorageStateStore, type User, type UserManagerSettings } from 'oidc-client-ts';

// Keycloak OIDC Configuration
const keycloakConfig: UserManagerSettings = {
  authority: import.meta.env.VITE_KEYCLOAK_AUTHORITY || 'http://localhost:8180/realms/ai-agent',
  client_id: import.meta.env.VITE_KEYCLOAK_CLIENT_ID || 'ai-agent-app',
  redirect_uri: `${window.location.origin}/callback`,
  post_logout_redirect_uri: `${window.location.origin}/`,
  response_type: 'code',
  scope: 'openid profile email',
  automaticSilentRenew: true,
  silent_redirect_uri: `${window.location.origin}/silent-renew.html`,
  userStore: new WebStorageStateStore({ store: window.localStorage }),
};

class AuthService {
  private userManager: UserManager;

  constructor() {
    this.userManager = new UserManager(keycloakConfig);

    // Event handlers
    this.userManager.events.addUserLoaded((user) => {
      console.log('User loaded:', user.profile);
    });

    this.userManager.events.addUserUnloaded(() => {
      console.log('User unloaded');
    });

    this.userManager.events.addAccessTokenExpiring(() => {
      console.log('Access token expiring...');
    });

    this.userManager.events.addSilentRenewError((error) => {
      console.error('Silent renew error:', error);
    });
  }

  /**
   * Redirect to Keycloak login page
   */
  async login(): Promise<void> {
    try {
      await this.userManager.signinRedirect();
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  }

  /**
   * Handle the callback from Keycloak after login
   */
  async handleCallback(): Promise<User> {
    try {
      const user = await this.userManager.signinRedirectCallback();
      return user;
    } catch (error) {
      console.error('Callback error:', error);
      throw error;
    }
  }

  /**
   * Logout and redirect to Keycloak
   */
  async logout(): Promise<void> {
    try {
      await this.userManager.signoutRedirect();
    } catch (error) {
      console.error('Logout error:', error);
      throw error;
    }
  }

  /**
   * Get the current authenticated user
   */
  async getUser(): Promise<User | null> {
    return await this.userManager.getUser();
  }

  /**
   * Get access token for API calls
   */
  async getAccessToken(): Promise<string | null> {
    const user = await this.getUser();
    return user?.access_token ?? null;
  }

  /**
   * Check if user is authenticated
   */
  async isAuthenticated(): Promise<boolean> {
    const user = await this.getUser();
    return user !== null && !user.expired;
  }

  /**
   * Get user roles from Keycloak token
   */
  async getUserRoles(): Promise<string[]> {
    const user = await this.getUser();
    if (!user?.profile) return [];
    
    // Keycloak stores roles in realm_access.roles
    const realmAccess = (user.profile as any).realm_access;
    return realmAccess?.roles ?? [];
  }

  /**
   * Silent renew token
   */
  async renewToken(): Promise<User | null> {
    try {
      return await this.userManager.signinSilent();
    } catch (error) {
      console.error('Token renewal error:', error);
      return null;
    }
  }
}

export const authService = new AuthService();
export default authService;
