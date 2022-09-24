import { Observable } from "rxjs/internal/Observable";

export class UserGeneratedPasswordModel {
  private _id: string;
  private _userId: string;
  private _password: string;
  private _expiresAt: Date;
  private _createdAt: Date;
  private _expiresInSeconds: number = 0;

  private _time: Observable<Date> = new Observable<Date>(observer => {
    setInterval(() => observer.next(new Date(Date.now())), 1000);
  });

  public constructor(
    id: string,
    userId: string,
    password: string,
    expiresAt: Date,
    createdAt: Date
  ) {

    this._id = id;
    this._userId = userId;
    this._password = password;
    this._expiresAt = expiresAt;
    this._createdAt = createdAt;

    this._computePasswordExpiration(new Date(Date.now()));

    this._time.subscribe((now: Date) => {
      this._computePasswordExpiration(now);
    });
  }

  public get id(): string {
    return this._id;
  }

  public get userId(): string {
    return this._userId;
  }

  public get password(): string {
    return this._password;
  }

  public get expiresAt(): Date {
    return this._expiresAt;
  }

  public get createdAt(): Date {
    return this._createdAt;
  }

  public get isExpired(): boolean {
    return this._expiresInSeconds <= 0;
  }

  public get expiresInSeconds(): number {
    return this._expiresInSeconds;
  }

  private _computePasswordExpiration(now: Date) {
    const expirationDate = new Date(this.expiresAt);
    const passwordExpired = expirationDate < now;
    this._expiresInSeconds = passwordExpired ? 0 : expirationDate.getTime() - now.getTime();
  }
}
