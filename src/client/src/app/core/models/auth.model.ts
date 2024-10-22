export interface LoginRequest {
    username: string;
    password: string;
}


export interface LoginResponse {
    token: string;
    expiresAt: string;
    user: User;
}

export interface User {
    email: string;
    username: string;
    createdAt: string;
}