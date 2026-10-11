// Altura del terreno (m) en cada punto: finca plana, ladera hacia el frente, lomas a los lados y montaña al fondo.
const X0 = -70, X1 = 75, Y0 = -75, Y1 = 62, BASE = -0.30;
function h(x, y) {
  const dx = Math.max(X0 - x, 0, x - X1), dyB = Math.max(y - Y1, 0), dyF = Math.max(Y0 - y, 0);
  if (dx === 0 && dyB === 0 && dyF === 0) return BASE;
  let z = BASE + 0.10 * dx + 0.15 * dyB + 0.0012 * dyB * dyB - 0.10 * dyF;
  z += 2.0 * Math.sin(x / 23) * Math.cos(y / 31) * Math.min(1, (dx + dyB + dyF) / 20); // ondulación natural
  return Math.round(z * 100) / 100;
}
module.exports = { h, X0, X1, Y0, Y1, BASE };
