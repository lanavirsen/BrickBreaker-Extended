let dotnetRef;
let keyDownHandler;
let keyUpHandler;

export function attachInputHandlers(reference) {
  detachInputHandlers();
  dotnetRef = reference;

  keyDownHandler = event => handleKeyEvent(event, true);
  keyUpHandler = event => handleKeyEvent(event, false);

  window.addEventListener("keydown", keyDownHandler);
  window.addEventListener("keyup", keyUpHandler);
}

export function detachInputHandlers() {
  if (keyDownHandler) {
    window.removeEventListener("keydown", keyDownHandler);
    keyDownHandler = undefined;
  }

  if (keyUpHandler) {
    window.removeEventListener("keyup", keyUpHandler);
    keyUpHandler = undefined;
  }

  dotnetRef = undefined;
}

function handleKeyEvent(event, isKeyDown) {
  if (!dotnetRef || shouldIgnoreEvent(event)) {
    return;
  }

  event.preventDefault();
  const method = isKeyDown ? "HandleGlobalKeyDown" : "HandleGlobalKeyUp";
  dotnetRef.invokeMethodAsync(method, event.key);
}

function shouldIgnoreEvent(event) {
  const active = document.activeElement;
  if (!active || active === document.body) {
    return false;
  }

  if (active.isContentEditable) {
    return true;
  }

  const tag = active.tagName;
  const isFormField = tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT" || tag === "BUTTON";
  if (isFormField && active.closest(".auth-grid")) {
    return true;
  }

  return false;
}
