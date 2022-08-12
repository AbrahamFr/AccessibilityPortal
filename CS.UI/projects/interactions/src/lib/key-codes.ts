import {
    END,
    ENTER,
    ESCAPE,
    HOME,
    DOWN_ARROW,
    UP_ARROW,
    LEFT_ARROW,
    RIGHT_ARROW,
    SPACE,
    TAB
  } from "@angular/cdk/keycodes";

export const keyCodes = {
    DOWN_ARROW,
    UP_ARROW,
    RIGHT_ARROW,
    LEFT_ARROW,
    SPACE,
    ENTER,
    ESCAPE,
    TAB,
    HOME,
    END
  };

  export function supportedKeyCode(keyCode: number): boolean {
    return Object.values(keyCodes).includes(keyCode);
  }