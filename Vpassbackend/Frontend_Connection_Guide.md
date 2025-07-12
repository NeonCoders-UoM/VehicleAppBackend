# Frontend Connection Examples

This document provides code examples for connecting your frontend application to the Vehicle Passport API backend.

## API Base URL

For local development: `http://localhost:5039`

## Authentication Examples

### React Example (using Axios)

```jsx
// api/auth.js
import axios from "axios";

const API_URL = "http://localhost:5039/api";

// Create an axios instance
const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Add a request interceptor to include auth token in all requests
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    if (token) {
      config.headers["Authorization"] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export const login = async (email, password) => {
  try {
    const response = await apiClient.post("/Auth/login", { email, password });
    if (response.data.token) {
      localStorage.setItem("token", response.data.token);
    }
    return response.data;
  } catch (error) {
    throw error.response
      ? error.response.data
      : new Error("Authentication failed");
  }
};

export const loginCustomer = async (email, password) => {
  try {
    const response = await apiClient.post("/Auth/login-customer", {
      email,
      password,
    });
    if (response.data.token) {
      localStorage.setItem("token", response.data.token);
    }
    return response.data;
  } catch (error) {
    throw error.response
      ? error.response.data
      : new Error("Customer authentication failed");
  }
};

export const registerCustomer = async (customerData) => {
  try {
    const response = await apiClient.post(
      "/Auth/register-customer",
      customerData
    );
    return response.data;
  } catch (error) {
    throw error.response
      ? error.response.data
      : new Error("Customer registration failed");
  }
};

export const verifyOtp = async (email, otp) => {
  try {
    const response = await apiClient.post("/Auth/verify-otp", { email, otp });
    return response.data;
  } catch (error) {
    throw error.response
      ? error.response.data
      : new Error("OTP verification failed");
  }
};

export const logout = () => {
  localStorage.removeItem("token");
};
```

### Angular Example

```typescript
// auth.service.ts
import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable, BehaviorSubject, throwError } from "rxjs";
import { catchError, tap } from "rxjs/operators";

@Injectable({
  providedIn: "root",
})
export class AuthService {
  private apiUrl = "http://localhost:5039/api/Auth";
  private currentUserSubject: BehaviorSubject<any> = new BehaviorSubject<any>(
    null
  );
  public currentUser = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    const token = localStorage.getItem("token");
    if (token) {
      // Decode JWT to get user info or make an API call to get user details
      // For simplicity, we're just setting a placeholder value
      this.currentUserSubject.next({ token });
    }
  }

  login(email: string, password: string): Observable<any> {
    return this.http
      .post<any>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        tap((response) => {
          if (response.token) {
            localStorage.setItem("token", response.token);
            this.currentUserSubject.next(response);
          }
        }),
        catchError((error) => {
          return throwError(() => new Error(error.error || "Login failed"));
        })
      );
  }

  loginCustomer(email: string, password: string): Observable<any> {
    return this.http
      .post<any>(`${this.apiUrl}/login-customer`, { email, password })
      .pipe(
        tap((response) => {
          if (response.token) {
            localStorage.setItem("token", response.token);
            this.currentUserSubject.next(response);
          }
        }),
        catchError((error) => {
          return throwError(
            () => new Error(error.error || "Customer login failed")
          );
        })
      );
  }

  registerCustomer(customerData: any): Observable<any> {
    return this.http
      .post<any>(`${this.apiUrl}/register-customer`, customerData)
      .pipe(
        catchError((error) => {
          return throwError(
            () => new Error(error.error || "Registration failed")
          );
        })
      );
  }

  verifyOtp(email: string, otp: string): Observable<any> {
    return this.http
      .post<any>(`${this.apiUrl}/verify-otp`, { email, otp })
      .pipe(
        catchError((error) => {
          return throwError(
            () => new Error(error.error || "OTP verification failed")
          );
        })
      );
  }

  logout(): void {
    localStorage.removeItem("token");
    this.currentUserSubject.next(null);
  }

  getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem("token");
    return new HttpHeaders({
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    });
  }
}
```

### Vue Example

```javascript
// auth.js
import axios from "axios";

const API_URL = "http://localhost:5039/api";

// Create axios instance with base URL
const api = axios.create({
  baseURL: API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Add interceptor to include token in requests
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export default {
  login(email, password) {
    return api.post("/Auth/login", { email, password }).then((response) => {
      if (response.data.token) {
        localStorage.setItem("token", response.data.token);
      }
      return response.data;
    });
  },

  loginCustomer(email, password) {
    return api
      .post("/Auth/login-customer", { email, password })
      .then((response) => {
        if (response.data.token) {
          localStorage.setItem("token", response.data.token);
        }
        return response.data;
      });
  },

  registerCustomer(customerData) {
    return api.post("/Auth/register-customer", customerData);
  },

  verifyOtp(email, otp) {
    return api.post("/Auth/verify-otp", { email, otp });
  },

  logout() {
    localStorage.removeItem("token");
  },

  isAuthenticated() {
    return !!localStorage.getItem("token");
  },
};
```

## Fetching Data Examples

### React Example

```jsx
// api/services.js
import axios from "axios";

const API_URL = "http://localhost:5039/api";

const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    if (token) {
      config.headers["Authorization"] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export const getServices = async () => {
  try {
    const response = await apiClient.get("/Services");
    return response.data;
  } catch (error) {
    throw error.response
      ? error.response.data
      : new Error("Failed to fetch services");
  }
};

export const getServiceById = async (id) => {
  try {
    const response = await apiClient.get(`/Services/${id}`);
    return response.data;
  } catch (error) {
    throw error.response
      ? error.response.data
      : new Error("Failed to fetch service details");
  }
};

// Component using the API
import React, { useState, useEffect } from "react";
import { getServices } from "../api/services";

const ServicesPage = () => {
  const [services, setServices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchServices = async () => {
      try {
        const data = await getServices();
        setServices(data);
      } catch (err) {
        setError(err.message || "Failed to fetch services");
      } finally {
        setLoading(false);
      }
    };

    fetchServices();
  }, []);

  if (loading) return <div>Loading services...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div>
      <h1>Available Services</h1>
      <div className="services-grid">
        {services.map((service) => (
          <div key={service.serviceId} className="service-card">
            <h3>{service.serviceName}</h3>
            <p>{service.description}</p>
            <p>Price: ${service.basePrice}</p>
            <p>Category: {service.category}</p>
          </div>
        ))}
      </div>
    </div>
  );
};

export default ServicesPage;
```

## Handling Authentication Status

### React Context Example

```jsx
// AuthContext.js
import React, { createContext, useState, useContext, useEffect } from "react";
import { login, logout, loginCustomer } from "../api/auth";

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [currentUser, setCurrentUser] = useState(null);
  const [loading, setLoading] = useState(true);

  // Check if user is already logged in (has token)
  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) {
      // In a real app, you'd validate the token or fetch user details
      setCurrentUser({ token });
    }
    setLoading(false);
  }, []);

  // Login function
  const handleLogin = async (email, password) => {
    try {
      const userData = await login(email, password);
      setCurrentUser(userData);
      return userData;
    } catch (error) {
      throw error;
    }
  };

  // Customer login function
  const handleCustomerLogin = async (email, password) => {
    try {
      const userData = await loginCustomer(email, password);
      setCurrentUser(userData);
      return userData;
    } catch (error) {
      throw error;
    }
  };

  // Logout function
  const handleLogout = () => {
    logout();
    setCurrentUser(null);
  };

  const value = {
    currentUser,
    login: handleLogin,
    loginCustomer: handleCustomerLogin,
    logout: handleLogout,
    isAuthenticated: !!currentUser,
  };

  return (
    <AuthContext.Provider value={value}>
      {!loading && children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
```

## Common Troubleshooting Tips

1. **CORS Errors**: If you encounter CORS errors, make sure your frontend URL is included in the allowed origins list in the backend.

2. **401 Unauthorized**: Check if your token is being correctly sent in the Authorization header.

3. **Token Expiration**: If you suddenly get 401 errors after working for a while, your token may have expired.

4. **API URL Issues**: Double-check that you're using the correct API base URL.

5. **Network Problems**: Check if your backend server is running and accessible from your frontend application.

## Development Steps

1. Start the backend server:

   ```
   cd VehicleAppBackend/Vpassbackend
   dotnet run
   ```

2. Start your frontend development server (React, Angular, Vue, etc.)

3. Make API calls from your frontend to the backend endpoints

4. Always use the token-based authentication flow described in the examples above
