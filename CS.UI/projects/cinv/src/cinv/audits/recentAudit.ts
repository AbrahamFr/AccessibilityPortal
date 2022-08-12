export type RecentAudit = {
    scanName: string;
    startingUrl: string;
    healthScore: number;
    finished: Date;
    healthScoreChange: number;
    healthScoreChangePercent: number;
    checkpointGroupDescription: string;
    status: number;
    totalPages: number;
    levels: number;
  };