export class UserGeneratedPasswordModel {
  public id!: string;
  public userId!: string;
  public password!: string;
  public expiersAt!: Date;
  public createdAt!: Date;

  public get isExpired(): boolean {
    const now = new Date(Date.now());
    return now > new Date(this.expiersAt);
  }
}
