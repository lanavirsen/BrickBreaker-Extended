let canvasEl;
let ctx;
let baseWidth = 0;
let baseHeight = 0;
let resizeObserver;
let resizeFallback;

export function init(canvas, width, height) {
  canvasEl = canvas;
  ctx = canvas.getContext("2d");
  baseWidth = width;
  baseHeight = height;
  canvasEl.width = width;
  canvasEl.height = height;
  ctx.textBaseline = "middle";
  ctx.font = "14px 'Consolas', monospace";

  setupResizeObserver();
}

export function render(state) {
  if (!ctx || !canvasEl) {
    return;
  }

  const {
    playArea,
    paddle,
    balls,
    bricks,
    powerUps,
    scorePopups,
    overlay
  } = state;

  ctx.clearRect(0, 0, canvasEl.width, canvasEl.height);
  ctx.fillStyle = "#03030a";
  ctx.fillRect(0, 0, canvasEl.width, canvasEl.height);

  drawPlayArea(playArea, overlay);
  drawBricks(bricks);
  drawPowerUps(powerUps);
  drawBalls(balls);
  drawPaddle(paddle, overlay);
  drawPopups(scorePopups);
  drawOverlay(playArea, overlay);
}

function drawPlayArea(area, overlay) {
  ctx.save();
  ctx.lineWidth = 6;
  ctx.strokeStyle = `hsl(${overlay.borderHue}, 100%, 60%)`;
  ctx.strokeRect(area.x, area.y, area.width, area.height);
  ctx.fillStyle = "#11142a";
  ctx.fillRect(area.x, area.y, area.width, area.height);
  ctx.restore();
}

function drawBricks(bricks) {
  bricks.forEach(brick => {
    ctx.fillStyle = `rgb(${brick.color.r}, ${brick.color.g}, ${brick.color.b})`;
    ctx.fillRect(brick.x, brick.y, brick.width, brick.height);
    ctx.strokeStyle = "rgba(0,0,0,0.55)";
    ctx.strokeRect(brick.x, brick.y, brick.width, brick.height);
  });
}

function drawPowerUps(powerUps) {
  ctx.save();
  ctx.lineWidth = 2;
  powerUps.forEach(powerUp => {
    const centerX = powerUp.x + powerUp.width / 2;
    const centerY = powerUp.y + powerUp.height / 2;
    const isMultiball = powerUp.kind === "Multiball";

    ctx.beginPath();
    ctx.fillStyle = isMultiball ? "#ffe066" : "#8ef5ff";
    ctx.ellipse(centerX, centerY, powerUp.width / 2, powerUp.height / 2, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.strokeStyle = "#03030a";
    ctx.stroke();

    ctx.fillStyle = "#050308";
    ctx.font = "bold 16px 'Consolas', monospace";
    ctx.textAlign = "center";
    ctx.fillText(isMultiball ? "M" : "E", centerX, centerY + 1);
  });
  ctx.restore();
}

function drawBalls(balls) {
  ctx.save();
  ctx.fillStyle = "#6fe0ff";
  balls.forEach(ball => {
    ctx.beginPath();
    ctx.arc(ball.x + ball.radius, ball.y + ball.radius, ball.radius, 0, Math.PI * 2);
    ctx.fill();
  });
  ctx.restore();
}

function drawPaddle(paddle, overlay) {
  ctx.save();
  const baseColor = "#2ec4ff"; // keep a consistent teal regardless of launch state
  // Show a reddish flash while the extender power-up winds down
  ctx.fillStyle = overlay.paddleBlinking ? "#ff5c7a" : baseColor;
  ctx.fillRect(paddle.x, paddle.y, paddle.width, paddle.height);
  ctx.strokeStyle = "#081427";
  ctx.strokeRect(paddle.x, paddle.y, paddle.width, paddle.height);
  ctx.restore();
}

function drawPopups(popups) {
  ctx.save();
  ctx.font = "bold 16px 'Consolas', monospace";
  popups.forEach(popup => {
    ctx.globalAlpha = popup.opacity;
    ctx.fillStyle = popup.isMultiplier ? "#ff8d57" : "#ffe169";
    ctx.fillText(popup.text, popup.x, popup.y);
  });
  ctx.restore();
  ctx.globalAlpha = 1;
}

function drawOverlay(playArea, overlay) {
  if (!overlay.ballReady && !overlay.isPaused && !overlay.isGameOver) {
    return;
  }

  ctx.save();
  ctx.textAlign = "center";
  ctx.font = "bold 18px 'Consolas', monospace";
  const centerX = playArea.x + playArea.width / 2;
  const centerY = playArea.y + playArea.height / 2;

  if (overlay.isGameOver) {
    ctx.fillStyle = "rgba(0, 0, 0, 0.65)";
    ctx.fillRect(playArea.x, playArea.y, playArea.width, playArea.height);
    ctx.fillStyle = "#ff6b6b";
    ctx.fillText("Game Over", centerX, centerY - 10);
    ctx.fillStyle = "#f5f7ff";
    ctx.font = "14px 'Consolas', monospace";
    ctx.fillText("Press Enter to try again", centerX, centerY + 12);
  } else if (overlay.isPaused) {
    ctx.fillStyle = "rgba(12, 16, 31, 0.85)";
    ctx.fillRect(playArea.x, playArea.y, playArea.width, playArea.height);
    ctx.fillStyle = "#f5f7ff";
    ctx.fillText("Paused", centerX, centerY);
  } else if (overlay.ballReady) {
    ctx.fillStyle = "#f5f7ff";
    ctx.fillText("Press ↑ or W to launch", centerX, centerY);
  }

  ctx.restore();
}

function setupResizeObserver() {
  cleanupResizeObserver();
  if (!canvasEl) {
    return;
  }

  const container = canvasEl.parentElement;
  if (!container) {
    return;
  }

  const applyFromContainer = () => {
    const rect = container.getBoundingClientRect();
    updateCanvasCssSize(rect.width, rect.height);
  };

  if (typeof ResizeObserver === "function") {
    resizeObserver = new ResizeObserver(entries => {
      for (const entry of entries) {
        const { width, height } = entry.contentRect;
        updateCanvasCssSize(width, height);
      }
    });
    resizeObserver.observe(container);
    applyFromContainer();
  } else {
    resizeFallback = () => applyFromContainer();
    window.addEventListener("resize", resizeFallback);
    applyFromContainer();
  }
}

function updateCanvasCssSize(width, height) {
  if (!canvasEl || !width || !height || !baseWidth || !baseHeight) {
    return;
  }
  const scale = Math.min(width / baseWidth, height / baseHeight);
  const targetWidth = baseWidth * scale;
  const targetHeight = baseHeight * scale;
  canvasEl.style.width = `${targetWidth}px`;
  canvasEl.style.height = `${targetHeight}px`;
}

function cleanupResizeObserver() {
  if (resizeObserver) {
    resizeObserver.disconnect();
    resizeObserver = undefined;
  }

  if (resizeFallback) {
    window.removeEventListener("resize", resizeFallback);
    resizeFallback = undefined;
  }
}

export function dispose() {
  cleanupResizeObserver();
  canvasEl = undefined;
  ctx = undefined;
}
