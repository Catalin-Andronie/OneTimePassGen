import { UserGeneratedPasswordDto } from "./UserGeneratedPasswordDto";
import { UserGeneratedPasswordModel } from "./UserGeneratedPasswordModel";

export class UserGeneratedPasswordMapper {

  /**
   * Create new instance of @see @type {UserGeneratedPasswordModel[]} from an @see @type {UserGeneratedPasswordDto[]}.
   *
   * @static
   * @param {UserGeneratedPasswordDto[]} source The array of  DTO instances from which we need to map from.
   * @returns {UserGeneratedPasswordModel[]} The array of Model instances mapped from source.
   */
  public static mapUserGeneratedPasswordModelFromDTOs(source: UserGeneratedPasswordDto[]): UserGeneratedPasswordModel[] {
    const items = new Array<UserGeneratedPasswordModel>();

    if (!source || source.length === 0) { return items; }

    for (const item of source) {
      items.push(this.mapUserGeneratedPasswordModelFromDTO(item));
    }

    return items;
  }

  /**
   * Create new instance of @see @type {UserGeneratedPasswordModel} from an @see @type {UserGeneratedPasswordDto}.
   *
   * @static
   * @param {UserGeneratedPasswordDto} source The DTO instance from which we need to map from.
   * @returns {UserGeneratedPasswordModel} The Model instance mapped from source.
   */
  public static mapUserGeneratedPasswordModelFromDTO(source: UserGeneratedPasswordDto): UserGeneratedPasswordModel {
    if (!source)
      throw new Error('The mapping "source" cannot is null or empty.');
    if (!source.id)
      throw new Error('Value of "id" cannot be null or empty.');

    const result = new UserGeneratedPasswordModel(
      source.id,
      source.password,
      source.expiresAt,
      source.createdAt
    );

    return result;
  }
}
