export class ValidationException extends Error {
  readonly fieldName: string | null;

  constructor(message: string);
  constructor(fieldName: string, message: string);
  constructor(fieldNameOrMessage: string, message?: string) {
    if (message === undefined) {
      super(fieldNameOrMessage);
      this.fieldName = null;
    } else {
      super(message);
      this.fieldName = fieldNameOrMessage;
    }
  }
}
