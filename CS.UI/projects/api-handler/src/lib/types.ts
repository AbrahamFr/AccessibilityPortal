export type Scan = {
  scanId: string | number;
  displayName: string;
};

export type APIResponse = {
  errorCode: string;
  data: object;
}

export type TimeZone = {
  id: number;
  displayName: string;
}
