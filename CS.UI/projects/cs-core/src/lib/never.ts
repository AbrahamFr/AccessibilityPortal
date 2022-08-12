export function neverEver(thingThatNeverHappens: never): never {
  throw new Error("Expected to never happen, got: " + thingThatNeverHappens);
}
