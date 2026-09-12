/** Matches PhotographerApp.Core.Enums.VideoType — stored as a plain int column, not a Postgres enum. */
export enum VideoType {
  External = 0,
  Embedded = 1,
  Uploaded = 2,
}
