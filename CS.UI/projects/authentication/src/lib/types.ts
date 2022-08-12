export type Claims = {
  orgVirtualDir: string;
  organizationId: string;
  permissions: Permission[];
  userName: string;
  aud: string;
  exp: string;
  iss: string;
  roles: string[];
  scanGroupId: number;
  userGroupId: number;
};

export type Permission = {
  Action: string;
  Id: string;
  PermissionId: string;
  Type: string;
};

export type AuthRequest = {
  UserName: string;
  Password: string;
  OrganizationId: string;
  AuthenticationType: string;
};

export enum AuthMode {
  AuthWebForms,
  AuthWebAPI,
}

export type User = {
  userId: number;
  name: string;
  emailAddress: string;
  firstName: string;
  lastName: string;
  organizationId: string;
  timeZone: string;
};

export type ClientSettings = {
  publicUser: {
    userName: string;
    password: string;
  };
  organizationId: string;
  authenticationType: string;
  refreshTimeout: number;
};

export type ResetPasswordRequest = {
  userId: number;
  userName: string;
  temporaryPassword: string;
  newPassword: string;
  organizationId: string;
  verificationToken: string;
  authenticationType: string;
}

export type UpdatePasswordRequest = {
  currentPassword: string;
  newPassword: string;
}

export type UpdateUserRequest = {
  userId: number;
  userName: string;
  firstName: string;
  lastName: string;
  emailAddress: string;
  timeZone:string;
}
