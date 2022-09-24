export interface UserGeneratedPasswordModel {
  id: string;
  userId: string;
  password: string;
  expiersAt: Date;
  createdAt: Date;
}
