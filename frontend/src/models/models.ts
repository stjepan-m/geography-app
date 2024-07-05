export type DbTable = {
  [propKey: string]: string | number | boolean;
};

export type GameType = DbTable & {
  id: number;
  name: string;
  locationType: string;
  interactionType: string;
  featureType: string;
  isSequential: boolean;
  labelHr: string;
  labelEn: string;
  isActive: boolean;
};

export type Region = DbTable & {
  id: number;
  name: string;
  startLatitude: number;
  startLongitude: number;
  startZoom: number;
  labelHr: string;
  labelEn: string;
  isActive: boolean;
};

export type TimeLimitType = DbTable & {
  id: number;
  name: string;
  labelHr: string;
  labelEn: string;
  isDefault: boolean;
  isActive: boolean;
};

export type ScoringType = DbTable & {
  id: number;
  name: string;
  formula: string;
  maxScore: number;
  labelHr: string;
  labelEn: string;
  isActive: boolean;
};

export type Location = DbTable & {
  id: number;
  name: string;
  type: string;
  labelHr: string;
  labelEn: string;
  isCustom: boolean;
  regions: Array<RegionLocation>;
};

export type City = Location & {
  latitude: number;
  longitude: number;
  country: Country;
};

export type Country = Location & {
  landAndWaterCoordinates: string;
  countryCode: string;
};

export type RegionLocation = DbTable & {
  id: number;
  locationId: number;
  regionId: number;
};

export type Game = DbTable & {
  id: string;
  name: string;
  createdBy: number;
  type: number;
  scoringType: number;
  region: number;
  numberOfRounds: number;
  timeLimitSeconds: number;
  timeLimitType: number;
  allowSkip: boolean;
  allowRetry: boolean;
  isFinished: boolean;
  typeNavigation?: GameType;
  regionNavigation?: Region;
  scoringTypeNavigation?: ScoringType;
  timeLimitTypeNavigation?: TimeLimitType;
};

export type Round = DbTable & {
  [propKey: string]: string | number | boolean | Location;
  id: number;
  playerGameId: number;
  locationId: number;
  roundNumber: number;
  timeLeft: number;
  score: number;
  location: Location;
};

export type Player = DbTable & {
  id: number;
  nickname: string;
  userId?: string;
};

export type PlayerGame = DbTable & {
  id: number;
  playerId?: number;
  gameId?: string;
  roundsCompleted: number;
  timeLeft: number;
  totalScore: number;
  status: string;
  game?: Game;
  player?: Player;
  rounds?: Array<Round>;
};

export type TimeLimitTypesByGameTypeId = {
  [key: number]: TimeLimitType[]
}

export type ScoringTypesByGameTypeId = {
  [key: number]: ScoringType[]
}

export enum MessageType {
  Success,
  Error,
}
