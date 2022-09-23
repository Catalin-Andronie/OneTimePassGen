export interface UserGeneratedPassword {
  id: string;
  userId: string;
  password: string;
  expiersAt: Date;
  createdAt: Date;
}
