import { registerDecorator, ValidationOptions } from 'class-validator';

/** Mirrors FluentValidation's `NotEqual(x => x.OtherProperty)`. */
export function IsDifferentFrom(property: string, validationOptions?: ValidationOptions) {
  return function (object: object, propertyName: string) {
    registerDecorator({
      name: 'isDifferentFrom',
      target: object.constructor,
      propertyName,
      constraints: [property],
      options: validationOptions,
      validator: {
        validate(value: unknown, args?: { constraints: string[]; object: object }) {
          const [relatedProperty] = args!.constraints;
          const relatedValue = (args!.object as Record<string, unknown>)[relatedProperty];
          return value !== relatedValue;
        },
      },
    });
  };
}
