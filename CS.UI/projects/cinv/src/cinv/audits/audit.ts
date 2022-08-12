export type Audit = {
    scanId: number;
    runId: number;
    baseUrl: string;
    scanName: string;
    displayName: string;
    guideline: string;
    levels: number;
    pageLimit: number;
    totalPages: number;
    startingUrl: string;
    checkpointGroupDescription: string;
    checkpointGroupId: string;     
}