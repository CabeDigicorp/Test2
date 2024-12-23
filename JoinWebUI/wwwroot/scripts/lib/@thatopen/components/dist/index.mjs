var Fo = Object.defineProperty;
var So = (c, i, t) => i in c ? Fo(c, i, { enumerable: !0, configurable: !0, writable: !0, value: t }) : c[i] = t;
var C = (c, i, t) => (So(c, typeof i != "symbol" ? i + "" : i, t), t);
import * as D from "three";
import { BufferAttribute as jr, Vector3 as Q, Vector2 as ai, Plane as vn, Line3 as Ee, Triangle as ii, Sphere as Oo, Matrix4 as pe, Box3 as ae, BackSide as No, DoubleSide as qr, FrontSide as Zn, Mesh as rt, Ray as yo, Raycaster as Lo, Quaternion as de, Object3D as dn, Euler as Po, MeshBasicMaterial as Qr, LineBasicMaterial as _o, CylinderGeometry as Vt, BoxGeometry as Ut, BufferGeometry as jn, Float32BufferAttribute as qn, OctahedronGeometry as Gi, Line as Fe, SphereGeometry as wo, TorusGeometry as Ii, PlaneGeometry as Mo } from "three";
import * as Qt from "@thatopen/fragments";
import { FragmentsGroup as Ws, Serializer as Do } from "@thatopen/fragments";
import * as k from "web-ifc";
const Kr = 0, bo = 1, vo = 2, Qn = 2, Xs = 1.25, Kn = 1, Is = 6 * 4 + 4 + 4, Ys = 65535, Uo = Math.pow(2, -24), $s = Symbol("SKIP_GENERATION");
function xo(c) {
  return c.index ? c.index.count : c.attributes.position.count;
}
function li(c) {
  return xo(c) / 3;
}
function Bo(c, i = ArrayBuffer) {
  return c > 65535 ? new Uint32Array(new i(4 * c)) : new Uint16Array(new i(2 * c));
}
function Vo(c, i) {
  if (!c.index) {
    const t = c.attributes.position.count, e = i.useSharedArrayBuffer ? SharedArrayBuffer : ArrayBuffer, s = Bo(t, e);
    c.setIndex(new jr(s, 1));
    for (let n = 0; n < t; n++)
      s[n] = n;
  }
}
function Jr(c) {
  const i = li(c), t = c.drawRange, e = t.start / 3, s = (t.start + t.count) / 3, n = Math.max(0, e), r = Math.min(i, s) - n;
  return [{
    offset: Math.floor(n),
    count: Math.floor(r)
  }];
}
function to(c) {
  if (!c.groups || !c.groups.length)
    return Jr(c);
  const i = [], t = /* @__PURE__ */ new Set(), e = c.drawRange, s = e.start / 3, n = (e.start + e.count) / 3;
  for (const o of c.groups) {
    const a = o.start / 3, l = (o.start + o.count) / 3;
    t.add(Math.max(s, a)), t.add(Math.min(n, l));
  }
  const r = Array.from(t.values()).sort((o, a) => o - a);
  for (let o = 0; o < r.length - 1; o++) {
    const a = r[o], l = r[o + 1];
    i.push({
      offset: Math.floor(a),
      count: Math.floor(l - a)
    });
  }
  return i;
}
function Yo(c) {
  if (c.groups.length === 0)
    return !1;
  const i = li(c), t = to(c).sort((n, r) => n.offset - r.offset), e = t[t.length - 1];
  e.count = Math.min(i - e.offset, e.count);
  let s = 0;
  return t.forEach(({ count: n }) => s += n), i !== s;
}
function Nt(c, i, t) {
  return t.min.x = i[c], t.min.y = i[c + 1], t.min.z = i[c + 2], t.max.x = i[c + 3], t.max.y = i[c + 4], t.max.z = i[c + 5], t;
}
function Go(c) {
  c[0] = c[1] = c[2] = 1 / 0, c[3] = c[4] = c[5] = -1 / 0;
}
function Jn(c) {
  let i = -1, t = -1 / 0;
  for (let e = 0; e < 3; e++) {
    const s = c[e + 3] - c[e];
    s > t && (t = s, i = e);
  }
  return i;
}
function tr(c, i) {
  i.set(c);
}
function er(c, i, t) {
  let e, s;
  for (let n = 0; n < 3; n++) {
    const r = n + 3;
    e = c[n], s = i[n], t[n] = e < s ? e : s, e = c[r], s = i[r], t[r] = e > s ? e : s;
  }
}
function zi(c, i, t) {
  for (let e = 0; e < 3; e++) {
    const s = i[c + 2 * e], n = i[c + 2 * e + 1], r = s - n, o = s + n;
    r < t[e] && (t[e] = r), o > t[e + 3] && (t[e + 3] = o);
  }
}
function di(c) {
  const i = c[3] - c[0], t = c[4] - c[1], e = c[5] - c[2];
  return 2 * (i * t + t * e + e * i);
}
function Zs(c, i, t, e, s = null) {
  let n = 1 / 0, r = 1 / 0, o = 1 / 0, a = -1 / 0, l = -1 / 0, h = -1 / 0, f = 1 / 0, I = 1 / 0, u = 1 / 0, d = -1 / 0, E = -1 / 0, T = -1 / 0;
  const p = s !== null;
  for (let R = i * 6, S = (i + t) * 6; R < S; R += 6) {
    const m = c[R + 0], F = c[R + 1], O = m - F, y = m + F;
    O < n && (n = O), y > a && (a = y), p && m < f && (f = m), p && m > d && (d = m);
    const w = c[R + 2], L = c[R + 3], b = w - L, Y = w + L;
    b < r && (r = b), Y > l && (l = Y), p && w < I && (I = w), p && w > E && (E = w);
    const N = c[R + 4], M = c[R + 5], g = N - M, v = N + M;
    g < o && (o = g), v > h && (h = v), p && N < u && (u = N), p && N > T && (T = N);
  }
  e[0] = n, e[1] = r, e[2] = o, e[3] = a, e[4] = l, e[5] = h, p && (s[0] = f, s[1] = I, s[2] = u, s[3] = d, s[4] = E, s[5] = T);
}
function zo(c, i, t, e) {
  let s = 1 / 0, n = 1 / 0, r = 1 / 0, o = -1 / 0, a = -1 / 0, l = -1 / 0;
  for (let h = i * 6, f = (i + t) * 6; h < f; h += 6) {
    const I = c[h + 0];
    I < s && (s = I), I > o && (o = I);
    const u = c[h + 2];
    u < n && (n = u), u > a && (a = u);
    const d = c[h + 4];
    d < r && (r = d), d > l && (l = d);
  }
  e[0] = s, e[1] = n, e[2] = r, e[3] = o, e[4] = a, e[5] = l;
}
function ko(c, i) {
  Go(i);
  const t = c.attributes.position, e = c.index ? c.index.array : null, s = li(c), n = new Float32Array(s * 6), r = t.normalized, o = t.array, a = t.offset || 0;
  let l = 3;
  t.isInterleavedBufferAttribute && (l = t.data.stride);
  const h = ["getX", "getY", "getZ"];
  for (let f = 0; f < s; f++) {
    const I = f * 3, u = f * 6;
    let d = I + 0, E = I + 1, T = I + 2;
    e && (d = e[d], E = e[E], T = e[T]), r || (d = d * l + a, E = E * l + a, T = T * l + a);
    for (let p = 0; p < 3; p++) {
      let R, S, m;
      r ? (R = t[h[p]](d), S = t[h[p]](E), m = t[h[p]](T)) : (R = o[d + p], S = o[E + p], m = o[T + p]);
      let F = R;
      S < F && (F = S), m < F && (F = m);
      let O = R;
      S > O && (O = S), m > O && (O = m);
      const y = (O - F) / 2, w = p * 2;
      n[u + w + 0] = F + y, n[u + w + 1] = y + (Math.abs(F) + y) * Uo, F < i[p] && (i[p] = F), O > i[p + 3] && (i[p + 3] = O);
    }
  }
  return n;
}
const Te = 32, Ho = (c, i) => c.candidate - i.candidate, Se = new Array(Te).fill().map(() => ({
  count: 0,
  bounds: new Float32Array(6),
  rightCacheBounds: new Float32Array(6),
  leftCacheBounds: new Float32Array(6),
  candidate: 0
})), ki = new Float32Array(6);
function Wo(c, i, t, e, s, n) {
  let r = -1, o = 0;
  if (n === Kr)
    r = Jn(i), r !== -1 && (o = (i[r] + i[r + 3]) / 2);
  else if (n === bo)
    r = Jn(c), r !== -1 && (o = Xo(t, e, s, r));
  else if (n === vo) {
    const a = di(c);
    let l = Xs * s;
    const h = e * 6, f = (e + s) * 6;
    for (let I = 0; I < 3; I++) {
      const u = i[I], T = (i[I + 3] - u) / Te;
      if (s < Te / 4) {
        const p = [...Se];
        p.length = s;
        let R = 0;
        for (let m = h; m < f; m += 6, R++) {
          const F = p[R];
          F.candidate = t[m + 2 * I], F.count = 0;
          const {
            bounds: O,
            leftCacheBounds: y,
            rightCacheBounds: w
          } = F;
          for (let L = 0; L < 3; L++)
            w[L] = 1 / 0, w[L + 3] = -1 / 0, y[L] = 1 / 0, y[L + 3] = -1 / 0, O[L] = 1 / 0, O[L + 3] = -1 / 0;
          zi(m, t, O);
        }
        p.sort(Ho);
        let S = s;
        for (let m = 0; m < S; m++) {
          const F = p[m];
          for (; m + 1 < S && p[m + 1].candidate === F.candidate; )
            p.splice(m + 1, 1), S--;
        }
        for (let m = h; m < f; m += 6) {
          const F = t[m + 2 * I];
          for (let O = 0; O < S; O++) {
            const y = p[O];
            F >= y.candidate ? zi(m, t, y.rightCacheBounds) : (zi(m, t, y.leftCacheBounds), y.count++);
          }
        }
        for (let m = 0; m < S; m++) {
          const F = p[m], O = F.count, y = s - F.count, w = F.leftCacheBounds, L = F.rightCacheBounds;
          let b = 0;
          O !== 0 && (b = di(w) / a);
          let Y = 0;
          y !== 0 && (Y = di(L) / a);
          const N = Kn + Xs * (b * O + Y * y);
          N < l && (r = I, l = N, o = F.candidate);
        }
      } else {
        for (let S = 0; S < Te; S++) {
          const m = Se[S];
          m.count = 0, m.candidate = u + T + S * T;
          const F = m.bounds;
          for (let O = 0; O < 3; O++)
            F[O] = 1 / 0, F[O + 3] = -1 / 0;
        }
        for (let S = h; S < f; S += 6) {
          let O = ~~((t[S + 2 * I] - u) / T);
          O >= Te && (O = Te - 1);
          const y = Se[O];
          y.count++, zi(S, t, y.bounds);
        }
        const p = Se[Te - 1];
        tr(p.bounds, p.rightCacheBounds);
        for (let S = Te - 2; S >= 0; S--) {
          const m = Se[S], F = Se[S + 1];
          er(m.bounds, F.rightCacheBounds, m.rightCacheBounds);
        }
        let R = 0;
        for (let S = 0; S < Te - 1; S++) {
          const m = Se[S], F = m.count, O = m.bounds, w = Se[S + 1].rightCacheBounds;
          F !== 0 && (R === 0 ? tr(O, ki) : er(O, ki, ki)), R += F;
          let L = 0, b = 0;
          R !== 0 && (L = di(ki) / a);
          const Y = s - R;
          Y !== 0 && (b = di(w) / a);
          const N = Kn + Xs * (L * R + b * Y);
          N < l && (r = I, l = N, o = m.candidate);
        }
      }
    }
  } else
    console.warn(`MeshBVH: Invalid build strategy value ${n} used.`);
  return { axis: r, pos: o };
}
function Xo(c, i, t, e) {
  let s = 0;
  for (let n = i, r = i + t; n < r; n++)
    s += c[n * 6 + e * 2];
  return s / t;
}
class Hi {
  constructor() {
  }
}
function $o(c, i, t, e, s, n) {
  let r = e, o = e + s - 1;
  const a = n.pos, l = n.axis * 2;
  for (; ; ) {
    for (; r <= o && t[r * 6 + l] < a; )
      r++;
    for (; r <= o && t[o * 6 + l] >= a; )
      o--;
    if (r < o) {
      for (let h = 0; h < 3; h++) {
        let f = i[r * 3 + h];
        i[r * 3 + h] = i[o * 3 + h], i[o * 3 + h] = f;
      }
      for (let h = 0; h < 6; h++) {
        let f = t[r * 6 + h];
        t[r * 6 + h] = t[o * 6 + h], t[o * 6 + h] = f;
      }
      r++, o--;
    } else
      return r;
  }
}
function Zo(c, i, t, e, s, n) {
  let r = e, o = e + s - 1;
  const a = n.pos, l = n.axis * 2;
  for (; ; ) {
    for (; r <= o && t[r * 6 + l] < a; )
      r++;
    for (; r <= o && t[o * 6 + l] >= a; )
      o--;
    if (r < o) {
      let h = c[r];
      c[r] = c[o], c[o] = h;
      for (let f = 0; f < 6; f++) {
        let I = t[r * 6 + f];
        t[r * 6 + f] = t[o * 6 + f], t[o * 6 + f] = I;
      }
      r++, o--;
    } else
      return r;
  }
}
function jo(c, i) {
  const t = (c.index ? c.index.count : c.attributes.position.count) / 3, e = t > 2 ** 16, s = e ? 4 : 2, n = i ? new SharedArrayBuffer(t * s) : new ArrayBuffer(t * s), r = e ? new Uint32Array(n) : new Uint16Array(n);
  for (let o = 0, a = r.length; o < a; o++)
    r[o] = o;
  return r;
}
function qo(c, i) {
  const t = c.geometry, e = t.index ? t.index.array : null, s = i.maxDepth, n = i.verbose, r = i.maxLeafTris, o = i.strategy, a = i.onProgress, l = li(t), h = c._indirectBuffer;
  let f = !1;
  const I = new Float32Array(6), u = new Float32Array(6), d = ko(t, I), E = i.indirect ? Zo : $o, T = [], p = i.indirect ? Jr(t) : to(t);
  if (p.length === 1) {
    const m = p[0], F = new Hi();
    F.boundingData = I, zo(d, m.offset, m.count, u), S(F, m.offset, m.count, u), T.push(F);
  } else
    for (let m of p) {
      const F = new Hi();
      F.boundingData = new Float32Array(6), Zs(d, m.offset, m.count, F.boundingData, u), S(F, m.offset, m.count, u), T.push(F);
    }
  return T;
  function R(m) {
    a && a(m / l);
  }
  function S(m, F, O, y = null, w = 0) {
    if (!f && w >= s && (f = !0, n && (console.warn(`MeshBVH: Max depth of ${s} reached when generating BVH. Consider increasing maxDepth.`), console.warn(t))), O <= r || w >= s)
      return R(F + O), m.offset = F, m.count = O, m;
    const L = Wo(m.boundingData, y, d, F, O, o);
    if (L.axis === -1)
      return R(F + O), m.offset = F, m.count = O, m;
    const b = E(h, e, d, F, O, L);
    if (b === F || b === F + O)
      R(F + O), m.offset = F, m.count = O;
    else {
      m.splitAxis = L.axis;
      const Y = new Hi(), N = F, M = b - F;
      m.left = Y, Y.boundingData = new Float32Array(6), Zs(d, N, M, Y.boundingData, u), S(Y, N, M, u, w + 1);
      const g = new Hi(), v = b, q = O - M;
      m.right = g, g.boundingData = new Float32Array(6), Zs(d, v, q, g.boundingData, u), S(g, v, q, u, w + 1);
    }
    return m;
  }
}
function Qo(c, i) {
  const t = c.geometry;
  i.indirect && (c._indirectBuffer = jo(t, i.useSharedArrayBuffer), Yo(t) && !i.verbose && console.warn(
    'MeshBVH: Provided geometry contains groups that do not fully span the vertex contents while using the "indirect" option. BVH may incorrectly report intersections on unrendered portions of the geometry.'
  )), c._indirectBuffer || Vo(t, i);
  const e = qo(c, i);
  let s, n, r;
  const o = [], a = i.useSharedArrayBuffer ? SharedArrayBuffer : ArrayBuffer;
  for (let f = 0; f < e.length; f++) {
    const I = e[f];
    let u = l(I);
    const d = new a(Is * u);
    s = new Float32Array(d), n = new Uint32Array(d), r = new Uint16Array(d), h(0, I), o.push(d);
  }
  c._roots = o;
  return;
  function l(f) {
    return f.count ? 1 : 1 + l(f.left) + l(f.right);
  }
  function h(f, I) {
    const u = f / 4, d = f / 2, E = !!I.count, T = I.boundingData;
    for (let p = 0; p < 6; p++)
      s[u + p] = T[p];
    if (E) {
      const p = I.offset, R = I.count;
      return n[u + 6] = p, r[d + 14] = R, r[d + 15] = Ys, f + Is;
    } else {
      const p = I.left, R = I.right, S = I.splitAxis;
      let m;
      if (m = h(f + Is, p), m / 4 > Math.pow(2, 32))
        throw new Error("MeshBVH: Cannot store child pointer greater than 32 bits.");
      return n[u + 6] = m / 4, m = h(m, R), n[u + 7] = S, m;
    }
  }
}
class Ae {
  constructor() {
    this.min = 1 / 0, this.max = -1 / 0;
  }
  setFromPointsField(i, t) {
    let e = 1 / 0, s = -1 / 0;
    for (let n = 0, r = i.length; n < r; n++) {
      const a = i[n][t];
      e = a < e ? a : e, s = a > s ? a : s;
    }
    this.min = e, this.max = s;
  }
  setFromPoints(i, t) {
    let e = 1 / 0, s = -1 / 0;
    for (let n = 0, r = t.length; n < r; n++) {
      const o = t[n], a = i.dot(o);
      e = a < e ? a : e, s = a > s ? a : s;
    }
    this.min = e, this.max = s;
  }
  isSeparated(i) {
    return this.min > i.max || i.min > this.max;
  }
}
Ae.prototype.setFromBox = function() {
  const c = new Q();
  return function(t, e) {
    const s = e.min, n = e.max;
    let r = 1 / 0, o = -1 / 0;
    for (let a = 0; a <= 1; a++)
      for (let l = 0; l <= 1; l++)
        for (let h = 0; h <= 1; h++) {
          c.x = s.x * a + n.x * (1 - a), c.y = s.y * l + n.y * (1 - l), c.z = s.z * h + n.z * (1 - h);
          const f = t.dot(c);
          r = Math.min(f, r), o = Math.max(f, o);
        }
    this.min = r, this.max = o;
  };
}();
const Ko = function() {
  const c = new Q(), i = new Q(), t = new Q();
  return function(s, n, r) {
    const o = s.start, a = c, l = n.start, h = i;
    t.subVectors(o, l), c.subVectors(s.end, s.start), i.subVectors(n.end, n.start);
    const f = t.dot(h), I = h.dot(a), u = h.dot(h), d = t.dot(a), T = a.dot(a) * u - I * I;
    let p, R;
    T !== 0 ? p = (f * I - d * u) / T : p = 0, R = (f + p * I) / u, r.x = p, r.y = R;
  };
}(), Un = function() {
  const c = new ai(), i = new Q(), t = new Q();
  return function(s, n, r, o) {
    Ko(s, n, c);
    let a = c.x, l = c.y;
    if (a >= 0 && a <= 1 && l >= 0 && l <= 1) {
      s.at(a, r), n.at(l, o);
      return;
    } else if (a >= 0 && a <= 1) {
      l < 0 ? n.at(0, o) : n.at(1, o), s.closestPointToPoint(o, !0, r);
      return;
    } else if (l >= 0 && l <= 1) {
      a < 0 ? s.at(0, r) : s.at(1, r), n.closestPointToPoint(r, !0, o);
      return;
    } else {
      let h;
      a < 0 ? h = s.start : h = s.end;
      let f;
      l < 0 ? f = n.start : f = n.end;
      const I = i, u = t;
      if (s.closestPointToPoint(f, !0, i), n.closestPointToPoint(h, !0, t), I.distanceToSquared(f) <= u.distanceToSquared(h)) {
        r.copy(I), o.copy(f);
        return;
      } else {
        r.copy(h), o.copy(u);
        return;
      }
    }
  };
}(), Jo = function() {
  const c = new Q(), i = new Q(), t = new vn(), e = new Ee();
  return function(n, r) {
    const { radius: o, center: a } = n, { a: l, b: h, c: f } = r;
    if (e.start = l, e.end = h, e.closestPointToPoint(a, !0, c).distanceTo(a) <= o || (e.start = l, e.end = f, e.closestPointToPoint(a, !0, c).distanceTo(a) <= o) || (e.start = h, e.end = f, e.closestPointToPoint(a, !0, c).distanceTo(a) <= o))
      return !0;
    const E = r.getPlane(t);
    if (Math.abs(E.distanceToPoint(a)) <= o) {
      const p = E.projectPoint(a, i);
      if (r.containsPoint(p))
        return !0;
    }
    return !1;
  };
}(), ta = 1e-15;
function js(c) {
  return Math.abs(c) < ta;
}
class ne extends ii {
  constructor(...i) {
    super(...i), this.isExtendedTriangle = !0, this.satAxes = new Array(4).fill().map(() => new Q()), this.satBounds = new Array(4).fill().map(() => new Ae()), this.points = [this.a, this.b, this.c], this.sphere = new Oo(), this.plane = new vn(), this.needsUpdate = !0;
  }
  intersectsSphere(i) {
    return Jo(i, this);
  }
  update() {
    const i = this.a, t = this.b, e = this.c, s = this.points, n = this.satAxes, r = this.satBounds, o = n[0], a = r[0];
    this.getNormal(o), a.setFromPoints(o, s);
    const l = n[1], h = r[1];
    l.subVectors(i, t), h.setFromPoints(l, s);
    const f = n[2], I = r[2];
    f.subVectors(t, e), I.setFromPoints(f, s);
    const u = n[3], d = r[3];
    u.subVectors(e, i), d.setFromPoints(u, s), this.sphere.setFromPoints(this.points), this.plane.setFromNormalAndCoplanarPoint(o, i), this.needsUpdate = !1;
  }
}
ne.prototype.closestPointToSegment = function() {
  const c = new Q(), i = new Q(), t = new Ee();
  return function(s, n = null, r = null) {
    const { start: o, end: a } = s, l = this.points;
    let h, f = 1 / 0;
    for (let I = 0; I < 3; I++) {
      const u = (I + 1) % 3;
      t.start.copy(l[I]), t.end.copy(l[u]), Un(t, s, c, i), h = c.distanceToSquared(i), h < f && (f = h, n && n.copy(c), r && r.copy(i));
    }
    return this.closestPointToPoint(o, c), h = o.distanceToSquared(c), h < f && (f = h, n && n.copy(c), r && r.copy(o)), this.closestPointToPoint(a, c), h = a.distanceToSquared(c), h < f && (f = h, n && n.copy(c), r && r.copy(a)), Math.sqrt(f);
  };
}();
ne.prototype.intersectsTriangle = function() {
  const c = new ne(), i = new Array(3), t = new Array(3), e = new Ae(), s = new Ae(), n = new Q(), r = new Q(), o = new Q(), a = new Q(), l = new Q(), h = new Ee(), f = new Ee(), I = new Ee(), u = new Q();
  function d(E, T, p) {
    const R = E.points;
    let S = 0, m = -1;
    for (let F = 0; F < 3; F++) {
      const { start: O, end: y } = h;
      O.copy(R[F]), y.copy(R[(F + 1) % 3]), h.delta(r);
      const w = js(T.distanceToPoint(O));
      if (js(T.normal.dot(r)) && w) {
        p.copy(h), S = 2;
        break;
      }
      const L = T.intersectLine(h, u);
      if (!L && w && u.copy(O), (L || w) && !js(u.distanceTo(y))) {
        if (S <= 1)
          (S === 1 ? p.start : p.end).copy(u), w && (m = S);
        else if (S >= 2) {
          (m === 1 ? p.start : p.end).copy(u), S = 2;
          break;
        }
        if (S++, S === 2 && m === -1)
          break;
      }
    }
    return S;
  }
  return function(T, p = null, R = !1) {
    this.needsUpdate && this.update(), T.isExtendedTriangle ? T.needsUpdate && T.update() : (c.copy(T), c.update(), T = c);
    const S = this.plane, m = T.plane;
    if (Math.abs(S.normal.dot(m.normal)) > 1 - 1e-10) {
      const F = this.satBounds, O = this.satAxes;
      t[0] = T.a, t[1] = T.b, t[2] = T.c;
      for (let L = 0; L < 4; L++) {
        const b = F[L], Y = O[L];
        if (e.setFromPoints(Y, t), b.isSeparated(e))
          return !1;
      }
      const y = T.satBounds, w = T.satAxes;
      i[0] = this.a, i[1] = this.b, i[2] = this.c;
      for (let L = 0; L < 4; L++) {
        const b = y[L], Y = w[L];
        if (e.setFromPoints(Y, i), b.isSeparated(e))
          return !1;
      }
      for (let L = 0; L < 4; L++) {
        const b = O[L];
        for (let Y = 0; Y < 4; Y++) {
          const N = w[Y];
          if (n.crossVectors(b, N), e.setFromPoints(n, i), s.setFromPoints(n, t), e.isSeparated(s))
            return !1;
        }
      }
      return p && (R || console.warn("ExtendedTriangle.intersectsTriangle: Triangles are coplanar which does not support an output edge. Setting edge to 0, 0, 0."), p.start.set(0, 0, 0), p.end.set(0, 0, 0)), !0;
    } else {
      const F = d(this, m, f);
      if (F === 1 && T.containsPoint(f.end))
        return p && (p.start.copy(f.end), p.end.copy(f.end)), !0;
      if (F !== 2)
        return !1;
      const O = d(T, S, I);
      if (O === 1 && this.containsPoint(I.end))
        return p && (p.start.copy(I.end), p.end.copy(I.end)), !0;
      if (O !== 2)
        return !1;
      if (f.delta(o), I.delta(a), o.dot(a) < 0) {
        let M = I.start;
        I.start = I.end, I.end = M;
      }
      const y = f.start.dot(o), w = f.end.dot(o), L = I.start.dot(o), b = I.end.dot(o), Y = w < L, N = y < b;
      return y !== b && L !== w && Y === N ? !1 : (p && (l.subVectors(f.start, I.start), l.dot(o) > 0 ? p.start.copy(f.start) : p.start.copy(I.start), l.subVectors(f.end, I.end), l.dot(o) < 0 ? p.end.copy(f.end) : p.end.copy(I.end)), !0);
    }
  };
}();
ne.prototype.distanceToPoint = function() {
  const c = new Q();
  return function(t) {
    return this.closestPointToPoint(t, c), t.distanceTo(c);
  };
}();
ne.prototype.distanceToTriangle = function() {
  const c = new Q(), i = new Q(), t = ["a", "b", "c"], e = new Ee(), s = new Ee();
  return function(r, o = null, a = null) {
    const l = o || a ? e : null;
    if (this.intersectsTriangle(r, l))
      return (o || a) && (o && l.getCenter(o), a && l.getCenter(a)), 0;
    let h = 1 / 0;
    for (let f = 0; f < 3; f++) {
      let I;
      const u = t[f], d = r[u];
      this.closestPointToPoint(d, c), I = d.distanceToSquared(c), I < h && (h = I, o && o.copy(c), a && a.copy(d));
      const E = this[u];
      r.closestPointToPoint(E, c), I = E.distanceToSquared(c), I < h && (h = I, o && o.copy(E), a && a.copy(c));
    }
    for (let f = 0; f < 3; f++) {
      const I = t[f], u = t[(f + 1) % 3];
      e.set(this[I], this[u]);
      for (let d = 0; d < 3; d++) {
        const E = t[d], T = t[(d + 1) % 3];
        s.set(r[E], r[T]), Un(e, s, c, i);
        const p = c.distanceToSquared(i);
        p < h && (h = p, o && o.copy(c), a && a.copy(i));
      }
    }
    return Math.sqrt(h);
  };
}();
class Bt {
  constructor(i, t, e) {
    this.isOrientedBox = !0, this.min = new Q(), this.max = new Q(), this.matrix = new pe(), this.invMatrix = new pe(), this.points = new Array(8).fill().map(() => new Q()), this.satAxes = new Array(3).fill().map(() => new Q()), this.satBounds = new Array(3).fill().map(() => new Ae()), this.alignedSatBounds = new Array(3).fill().map(() => new Ae()), this.needsUpdate = !1, i && this.min.copy(i), t && this.max.copy(t), e && this.matrix.copy(e);
  }
  set(i, t, e) {
    this.min.copy(i), this.max.copy(t), this.matrix.copy(e), this.needsUpdate = !0;
  }
  copy(i) {
    this.min.copy(i.min), this.max.copy(i.max), this.matrix.copy(i.matrix), this.needsUpdate = !0;
  }
}
Bt.prototype.update = /* @__PURE__ */ function() {
  return function() {
    const i = this.matrix, t = this.min, e = this.max, s = this.points;
    for (let l = 0; l <= 1; l++)
      for (let h = 0; h <= 1; h++)
        for (let f = 0; f <= 1; f++) {
          const I = 1 * l | 2 * h | 4 * f, u = s[I];
          u.x = l ? e.x : t.x, u.y = h ? e.y : t.y, u.z = f ? e.z : t.z, u.applyMatrix4(i);
        }
    const n = this.satBounds, r = this.satAxes, o = s[0];
    for (let l = 0; l < 3; l++) {
      const h = r[l], f = n[l], I = 1 << l, u = s[I];
      h.subVectors(o, u), f.setFromPoints(h, s);
    }
    const a = this.alignedSatBounds;
    a[0].setFromPointsField(s, "x"), a[1].setFromPointsField(s, "y"), a[2].setFromPointsField(s, "z"), this.invMatrix.copy(this.matrix).invert(), this.needsUpdate = !1;
  };
}();
Bt.prototype.intersectsBox = function() {
  const c = new Ae();
  return function(t) {
    this.needsUpdate && this.update();
    const e = t.min, s = t.max, n = this.satBounds, r = this.satAxes, o = this.alignedSatBounds;
    if (c.min = e.x, c.max = s.x, o[0].isSeparated(c) || (c.min = e.y, c.max = s.y, o[1].isSeparated(c)) || (c.min = e.z, c.max = s.z, o[2].isSeparated(c)))
      return !1;
    for (let a = 0; a < 3; a++) {
      const l = r[a], h = n[a];
      if (c.setFromBox(l, t), h.isSeparated(c))
        return !1;
    }
    return !0;
  };
}();
Bt.prototype.intersectsTriangle = function() {
  const c = new ne(), i = new Array(3), t = new Ae(), e = new Ae(), s = new Q();
  return function(r) {
    this.needsUpdate && this.update(), r.isExtendedTriangle ? r.needsUpdate && r.update() : (c.copy(r), c.update(), r = c);
    const o = this.satBounds, a = this.satAxes;
    i[0] = r.a, i[1] = r.b, i[2] = r.c;
    for (let I = 0; I < 3; I++) {
      const u = o[I], d = a[I];
      if (t.setFromPoints(d, i), u.isSeparated(t))
        return !1;
    }
    const l = r.satBounds, h = r.satAxes, f = this.points;
    for (let I = 0; I < 3; I++) {
      const u = l[I], d = h[I];
      if (t.setFromPoints(d, f), u.isSeparated(t))
        return !1;
    }
    for (let I = 0; I < 3; I++) {
      const u = a[I];
      for (let d = 0; d < 4; d++) {
        const E = h[d];
        if (s.crossVectors(u, E), t.setFromPoints(s, i), e.setFromPoints(s, f), t.isSeparated(e))
          return !1;
      }
    }
    return !0;
  };
}();
Bt.prototype.closestPointToPoint = /* @__PURE__ */ function() {
  return function(i, t) {
    return this.needsUpdate && this.update(), t.copy(i).applyMatrix4(this.invMatrix).clamp(this.min, this.max).applyMatrix4(this.matrix), t;
  };
}();
Bt.prototype.distanceToPoint = function() {
  const c = new Q();
  return function(t) {
    return this.closestPointToPoint(t, c), t.distanceTo(c);
  };
}();
Bt.prototype.distanceToBox = function() {
  const c = ["x", "y", "z"], i = new Array(12).fill().map(() => new Ee()), t = new Array(12).fill().map(() => new Ee()), e = new Q(), s = new Q();
  return function(r, o = 0, a = null, l = null) {
    if (this.needsUpdate && this.update(), this.intersectsBox(r))
      return (a || l) && (r.getCenter(s), this.closestPointToPoint(s, e), r.closestPointToPoint(e, s), a && a.copy(e), l && l.copy(s)), 0;
    const h = o * o, f = r.min, I = r.max, u = this.points;
    let d = 1 / 0;
    for (let T = 0; T < 8; T++) {
      const p = u[T];
      s.copy(p).clamp(f, I);
      const R = p.distanceToSquared(s);
      if (R < d && (d = R, a && a.copy(p), l && l.copy(s), R < h))
        return Math.sqrt(R);
    }
    let E = 0;
    for (let T = 0; T < 3; T++)
      for (let p = 0; p <= 1; p++)
        for (let R = 0; R <= 1; R++) {
          const S = (T + 1) % 3, m = (T + 2) % 3, F = p << S | R << m, O = 1 << T | p << S | R << m, y = u[F], w = u[O];
          i[E].set(y, w);
          const b = c[T], Y = c[S], N = c[m], M = t[E], g = M.start, v = M.end;
          g[b] = f[b], g[Y] = p ? f[Y] : I[Y], g[N] = R ? f[N] : I[Y], v[b] = I[b], v[Y] = p ? f[Y] : I[Y], v[N] = R ? f[N] : I[Y], E++;
        }
    for (let T = 0; T <= 1; T++)
      for (let p = 0; p <= 1; p++)
        for (let R = 0; R <= 1; R++) {
          s.x = T ? I.x : f.x, s.y = p ? I.y : f.y, s.z = R ? I.z : f.z, this.closestPointToPoint(s, e);
          const S = s.distanceToSquared(e);
          if (S < d && (d = S, a && a.copy(e), l && l.copy(s), S < h))
            return Math.sqrt(S);
        }
    for (let T = 0; T < 12; T++) {
      const p = i[T];
      for (let R = 0; R < 12; R++) {
        const S = t[R];
        Un(p, S, e, s);
        const m = e.distanceToSquared(s);
        if (m < d && (d = m, a && a.copy(e), l && l.copy(s), m < h))
          return Math.sqrt(m);
      }
    }
    return Math.sqrt(d);
  };
}();
class xn {
  constructor(i) {
    this._getNewPrimitive = i, this._primitives = [];
  }
  getPrimitive() {
    const i = this._primitives;
    return i.length === 0 ? this._getNewPrimitive() : i.pop();
  }
  releasePrimitive(i) {
    this._primitives.push(i);
  }
}
class ea extends xn {
  constructor() {
    super(() => new ne());
  }
}
const $t = /* @__PURE__ */ new ea();
function zt(c, i) {
  return i[c + 15] === 65535;
}
function kt(c, i) {
  return i[c + 6];
}
function Zt(c, i) {
  return i[c + 14];
}
function jt(c) {
  return c + 8;
}
function qt(c, i) {
  return i[c + 6];
}
function eo(c, i) {
  return i[c + 7];
}
class ia {
  constructor() {
    this.float32Array = null, this.uint16Array = null, this.uint32Array = null;
    const i = [];
    let t = null;
    this.setBuffer = (e) => {
      t && i.push(t), t = e, this.float32Array = new Float32Array(e), this.uint16Array = new Uint16Array(e), this.uint32Array = new Uint32Array(e);
    }, this.clearBuffer = () => {
      t = null, this.float32Array = null, this.uint16Array = null, this.uint32Array = null, i.length !== 0 && this.setBuffer(i.pop());
    };
  }
}
const Ft = new ia();
let Pe, si;
const ze = [], Wi = /* @__PURE__ */ new xn(() => new ae());
function sa(c, i, t, e, s, n) {
  Pe = Wi.getPrimitive(), si = Wi.getPrimitive(), ze.push(Pe, si), Ft.setBuffer(c._roots[i]);
  const r = En(0, c.geometry, t, e, s, n);
  Ft.clearBuffer(), Wi.releasePrimitive(Pe), Wi.releasePrimitive(si), ze.pop(), ze.pop();
  const o = ze.length;
  return o > 0 && (si = ze[o - 1], Pe = ze[o - 2]), r;
}
function En(c, i, t, e, s = null, n = 0, r = 0) {
  const { float32Array: o, uint16Array: a, uint32Array: l } = Ft;
  let h = c * 2;
  if (zt(h, a)) {
    const I = kt(c, l), u = Zt(h, a);
    return Nt(c, o, Pe), e(I, u, !1, r, n + c, Pe);
  } else {
    let b = function(N) {
      const { uint16Array: M, uint32Array: g } = Ft;
      let v = N * 2;
      for (; !zt(v, M); )
        N = jt(N), v = N * 2;
      return kt(N, g);
    }, Y = function(N) {
      const { uint16Array: M, uint32Array: g } = Ft;
      let v = N * 2;
      for (; !zt(v, M); )
        N = qt(N, g), v = N * 2;
      return kt(N, g) + Zt(v, M);
    };
    const I = jt(c), u = qt(c, l);
    let d = I, E = u, T, p, R, S;
    if (s && (R = Pe, S = si, Nt(d, o, R), Nt(E, o, S), T = s(R), p = s(S), p < T)) {
      d = u, E = I;
      const N = T;
      T = p, p = N, R = S;
    }
    R || (R = Pe, Nt(d, o, R));
    const m = zt(d * 2, a), F = t(R, m, T, r + 1, n + d);
    let O;
    if (F === Qn) {
      const N = b(d), g = Y(d) - N;
      O = e(N, g, !0, r + 1, n + d, R);
    } else
      O = F && En(
        d,
        i,
        t,
        e,
        s,
        n,
        r + 1
      );
    if (O)
      return !0;
    S = si, Nt(E, o, S);
    const y = zt(E * 2, a), w = t(S, y, p, r + 1, n + E);
    let L;
    if (w === Qn) {
      const N = b(E), g = Y(E) - N;
      L = e(N, g, !0, r + 1, n + E, S);
    } else
      L = w && En(
        E,
        i,
        t,
        e,
        s,
        n,
        r + 1
      );
    return !!L;
  }
}
const Ei = /* @__PURE__ */ new Q(), qs = /* @__PURE__ */ new Q();
function na(c, i, t = {}, e = 0, s = 1 / 0) {
  const n = e * e, r = s * s;
  let o = 1 / 0, a = null;
  if (c.shapecast(
    {
      boundsTraverseOrder: (h) => (Ei.copy(i).clamp(h.min, h.max), Ei.distanceToSquared(i)),
      intersectsBounds: (h, f, I) => I < o && I < r,
      intersectsTriangle: (h, f) => {
        h.closestPointToPoint(i, Ei);
        const I = i.distanceToSquared(Ei);
        return I < o && (qs.copy(Ei), o = I, a = f), I < n;
      }
    }
  ), o === 1 / 0)
    return null;
  const l = Math.sqrt(o);
  return t.point ? t.point.copy(qs) : t.point = qs.clone(), t.distance = l, t.faceIndex = a, t;
}
const ke = /* @__PURE__ */ new Q(), He = /* @__PURE__ */ new Q(), We = /* @__PURE__ */ new Q(), Xi = /* @__PURE__ */ new ai(), $i = /* @__PURE__ */ new ai(), Zi = /* @__PURE__ */ new ai(), ir = /* @__PURE__ */ new Q(), sr = /* @__PURE__ */ new Q(), nr = /* @__PURE__ */ new Q(), ji = /* @__PURE__ */ new Q();
function ra(c, i, t, e, s, n) {
  let r;
  return n === No ? r = c.intersectTriangle(e, t, i, !0, s) : r = c.intersectTriangle(i, t, e, n !== qr, s), r === null ? null : {
    distance: c.origin.distanceTo(s),
    point: s.clone()
  };
}
function oa(c, i, t, e, s, n, r, o, a) {
  ke.fromBufferAttribute(i, n), He.fromBufferAttribute(i, r), We.fromBufferAttribute(i, o);
  const l = ra(c, ke, He, We, ji, a);
  if (l) {
    e && (Xi.fromBufferAttribute(e, n), $i.fromBufferAttribute(e, r), Zi.fromBufferAttribute(e, o), l.uv = ii.getInterpolation(ji, ke, He, We, Xi, $i, Zi, new ai())), s && (Xi.fromBufferAttribute(s, n), $i.fromBufferAttribute(s, r), Zi.fromBufferAttribute(s, o), l.uv1 = ii.getInterpolation(ji, ke, He, We, Xi, $i, Zi, new ai())), t && (ir.fromBufferAttribute(t, n), sr.fromBufferAttribute(t, r), nr.fromBufferAttribute(t, o), l.normal = ii.getInterpolation(ji, ke, He, We, ir, sr, nr, new Q()), l.normal.dot(c.direction) > 0 && l.normal.multiplyScalar(-1));
    const h = {
      a: n,
      b: r,
      c: o,
      normal: new Q(),
      materialIndex: 0
    };
    ii.getNormal(ke, He, We, h.normal), l.face = h, l.faceIndex = n;
  }
  return l;
}
function Gs(c, i, t, e, s) {
  const n = e * 3;
  let r = n + 0, o = n + 1, a = n + 2;
  const l = c.index;
  c.index && (r = l.getX(r), o = l.getX(o), a = l.getX(a));
  const { position: h, normal: f, uv: I, uv1: u } = c.attributes, d = oa(t, h, f, I, u, r, o, a, i);
  return d ? (d.faceIndex = e, s && s.push(d), d) : null;
}
function wt(c, i, t, e) {
  const s = c.a, n = c.b, r = c.c;
  let o = i, a = i + 1, l = i + 2;
  t && (o = t.getX(o), a = t.getX(a), l = t.getX(l)), s.x = e.getX(o), s.y = e.getY(o), s.z = e.getZ(o), n.x = e.getX(a), n.y = e.getY(a), n.z = e.getZ(a), r.x = e.getX(l), r.y = e.getY(l), r.z = e.getZ(l);
}
function aa(c, i, t, e, s, n) {
  const { geometry: r, _indirectBuffer: o } = c;
  for (let a = e, l = e + s; a < l; a++)
    Gs(r, i, t, a, n);
}
function ca(c, i, t, e, s) {
  const { geometry: n, _indirectBuffer: r } = c;
  let o = 1 / 0, a = null;
  for (let l = e, h = e + s; l < h; l++) {
    let f;
    f = Gs(n, i, t, l), f && f.distance < o && (a = f, o = f.distance);
  }
  return a;
}
function la(c, i, t, e, s, n, r) {
  const { geometry: o } = t, { index: a } = o, l = o.attributes.position;
  for (let h = c, f = i + c; h < f; h++) {
    let I;
    if (I = h, wt(r, I * 3, a, l), r.needsUpdate = !0, e(r, I, s, n))
      return !0;
  }
  return !1;
}
function ha(c, i = null) {
  i && Array.isArray(i) && (i = new Set(i));
  const t = c.geometry, e = t.index ? t.index.array : null, s = t.attributes.position;
  let n, r, o, a, l = 0;
  const h = c._roots;
  for (let I = 0, u = h.length; I < u; I++)
    n = h[I], r = new Uint32Array(n), o = new Uint16Array(n), a = new Float32Array(n), f(0, l), l += n.byteLength;
  function f(I, u, d = !1) {
    const E = I * 2;
    if (o[E + 15] === Ys) {
      const p = r[I + 6], R = o[E + 14];
      let S = 1 / 0, m = 1 / 0, F = 1 / 0, O = -1 / 0, y = -1 / 0, w = -1 / 0;
      for (let L = 3 * p, b = 3 * (p + R); L < b; L++) {
        let Y = e[L];
        const N = s.getX(Y), M = s.getY(Y), g = s.getZ(Y);
        N < S && (S = N), N > O && (O = N), M < m && (m = M), M > y && (y = M), g < F && (F = g), g > w && (w = g);
      }
      return a[I + 0] !== S || a[I + 1] !== m || a[I + 2] !== F || a[I + 3] !== O || a[I + 4] !== y || a[I + 5] !== w ? (a[I + 0] = S, a[I + 1] = m, a[I + 2] = F, a[I + 3] = O, a[I + 4] = y, a[I + 5] = w, !0) : !1;
    } else {
      const p = I + 8, R = r[I + 6], S = p + u, m = R + u;
      let F = d, O = !1, y = !1;
      i ? F || (O = i.has(S), y = i.has(m), F = !O && !y) : (O = !0, y = !0);
      const w = F || O, L = F || y;
      let b = !1;
      w && (b = f(p, u, F));
      let Y = !1;
      L && (Y = f(R, u, F));
      const N = b || Y;
      if (N)
        for (let M = 0; M < 3; M++) {
          const g = p + M, v = R + M, q = a[g], G = a[g + 3], et = a[v], W = a[v + 3];
          a[I + M] = q < et ? q : et, a[I + M + 3] = G > W ? G : W;
        }
      return N;
    }
  }
}
const rr = /* @__PURE__ */ new ae();
function Me(c, i, t, e) {
  return Nt(c, i, rr), t.intersectBox(rr, e);
}
function ua(c, i, t, e, s, n) {
  const { geometry: r, _indirectBuffer: o } = c;
  for (let a = e, l = e + s; a < l; a++) {
    let h = o ? o[a] : a;
    Gs(r, i, t, h, n);
  }
}
function fa(c, i, t, e, s) {
  const { geometry: n, _indirectBuffer: r } = c;
  let o = 1 / 0, a = null;
  for (let l = e, h = e + s; l < h; l++) {
    let f;
    f = Gs(n, i, t, r ? r[l] : l), f && f.distance < o && (a = f, o = f.distance);
  }
  return a;
}
function Ia(c, i, t, e, s, n, r) {
  const { geometry: o } = t, { index: a } = o, l = o.attributes.position;
  for (let h = c, f = i + c; h < f; h++) {
    let I;
    if (I = t.resolveTriangleIndex(h), wt(r, I * 3, a, l), r.needsUpdate = !0, e(r, I, s, n))
      return !0;
  }
  return !1;
}
const or = /* @__PURE__ */ new Q();
function da(c, i, t, e, s) {
  Ft.setBuffer(c._roots[i]), pn(0, c, t, e, s), Ft.clearBuffer();
}
function pn(c, i, t, e, s) {
  const { float32Array: n, uint16Array: r, uint32Array: o } = Ft, a = c * 2;
  if (zt(a, r)) {
    const h = kt(c, o), f = Zt(a, r);
    aa(i, t, e, h, f, s);
  } else {
    const h = jt(c);
    Me(h, n, e, or) && pn(h, i, t, e, s);
    const f = qt(c, o);
    Me(f, n, e, or) && pn(f, i, t, e, s);
  }
}
const ar = /* @__PURE__ */ new Q(), Ea = ["x", "y", "z"];
function pa(c, i, t, e) {
  Ft.setBuffer(c._roots[i]);
  const s = Cn(0, c, t, e);
  return Ft.clearBuffer(), s;
}
function Cn(c, i, t, e) {
  const { float32Array: s, uint16Array: n, uint32Array: r } = Ft;
  let o = c * 2;
  if (zt(o, n)) {
    const l = kt(c, r), h = Zt(o, n);
    return ca(i, t, e, l, h);
  } else {
    const l = eo(c, r), h = Ea[l], I = e.direction[h] >= 0;
    let u, d;
    I ? (u = jt(c), d = qt(c, r)) : (u = qt(c, r), d = jt(c));
    const T = Me(u, s, e, ar) ? Cn(u, i, t, e) : null;
    if (T) {
      const S = T.point[h];
      if (I ? S <= s[d + l] : (
        // min bounding data
        S >= s[d + l + 3]
      ))
        return T;
    }
    const R = Me(d, s, e, ar) ? Cn(d, i, t, e) : null;
    return T && R ? T.distance <= R.distance ? T : R : T || R || null;
  }
}
const qi = /* @__PURE__ */ new ae(), Xe = /* @__PURE__ */ new ne(), $e = /* @__PURE__ */ new ne(), pi = /* @__PURE__ */ new pe(), cr = /* @__PURE__ */ new Bt(), Qi = /* @__PURE__ */ new Bt();
function Ca(c, i, t, e) {
  Ft.setBuffer(c._roots[i]);
  const s = Tn(0, c, t, e);
  return Ft.clearBuffer(), s;
}
function Tn(c, i, t, e, s = null) {
  const { float32Array: n, uint16Array: r, uint32Array: o } = Ft;
  let a = c * 2;
  if (s === null && (t.boundingBox || t.computeBoundingBox(), cr.set(t.boundingBox.min, t.boundingBox.max, e), s = cr), zt(a, r)) {
    const h = i.geometry, f = h.index, I = h.attributes.position, u = t.index, d = t.attributes.position, E = kt(c, o), T = Zt(a, r);
    if (pi.copy(e).invert(), t.boundsTree)
      return Nt(c, n, Qi), Qi.matrix.copy(pi), Qi.needsUpdate = !0, t.boundsTree.shapecast({
        intersectsBounds: (R) => Qi.intersectsBox(R),
        intersectsTriangle: (R) => {
          R.a.applyMatrix4(e), R.b.applyMatrix4(e), R.c.applyMatrix4(e), R.needsUpdate = !0;
          for (let S = E * 3, m = (T + E) * 3; S < m; S += 3)
            if (wt($e, S, f, I), $e.needsUpdate = !0, R.intersectsTriangle($e))
              return !0;
          return !1;
        }
      });
    for (let p = E * 3, R = (T + E) * 3; p < R; p += 3) {
      wt(Xe, p, f, I), Xe.a.applyMatrix4(pi), Xe.b.applyMatrix4(pi), Xe.c.applyMatrix4(pi), Xe.needsUpdate = !0;
      for (let S = 0, m = u.count; S < m; S += 3)
        if (wt($e, S, u, d), $e.needsUpdate = !0, Xe.intersectsTriangle($e))
          return !0;
    }
  } else {
    const h = c + 8, f = o[c + 6];
    return Nt(h, n, qi), !!(s.intersectsBox(qi) && Tn(h, i, t, e, s) || (Nt(f, n, qi), s.intersectsBox(qi) && Tn(f, i, t, e, s)));
  }
}
const Ki = /* @__PURE__ */ new pe(), Qs = /* @__PURE__ */ new Bt(), Ci = /* @__PURE__ */ new Bt(), Ta = /* @__PURE__ */ new Q(), ma = /* @__PURE__ */ new Q(), Ra = /* @__PURE__ */ new Q(), ga = /* @__PURE__ */ new Q();
function Aa(c, i, t, e = {}, s = {}, n = 0, r = 1 / 0) {
  i.boundingBox || i.computeBoundingBox(), Qs.set(i.boundingBox.min, i.boundingBox.max, t), Qs.needsUpdate = !0;
  const o = c.geometry, a = o.attributes.position, l = o.index, h = i.attributes.position, f = i.index, I = $t.getPrimitive(), u = $t.getPrimitive();
  let d = Ta, E = ma, T = null, p = null;
  s && (T = Ra, p = ga);
  let R = 1 / 0, S = null, m = null;
  return Ki.copy(t).invert(), Ci.matrix.copy(Ki), c.shapecast(
    {
      boundsTraverseOrder: (F) => Qs.distanceToBox(F),
      intersectsBounds: (F, O, y) => y < R && y < r ? (O && (Ci.min.copy(F.min), Ci.max.copy(F.max), Ci.needsUpdate = !0), !0) : !1,
      intersectsRange: (F, O) => {
        if (i.boundsTree)
          return i.boundsTree.shapecast({
            boundsTraverseOrder: (w) => Ci.distanceToBox(w),
            intersectsBounds: (w, L, b) => b < R && b < r,
            intersectsRange: (w, L) => {
              for (let b = w, Y = w + L; b < Y; b++) {
                wt(u, 3 * b, f, h), u.a.applyMatrix4(t), u.b.applyMatrix4(t), u.c.applyMatrix4(t), u.needsUpdate = !0;
                for (let N = F, M = F + O; N < M; N++) {
                  wt(I, 3 * N, l, a), I.needsUpdate = !0;
                  const g = I.distanceToTriangle(u, d, T);
                  if (g < R && (E.copy(d), p && p.copy(T), R = g, S = N, m = b), g < n)
                    return !0;
                }
              }
            }
          });
        {
          const y = li(i);
          for (let w = 0, L = y; w < L; w++) {
            wt(u, 3 * w, f, h), u.a.applyMatrix4(t), u.b.applyMatrix4(t), u.c.applyMatrix4(t), u.needsUpdate = !0;
            for (let b = F, Y = F + O; b < Y; b++) {
              wt(I, 3 * b, l, a), I.needsUpdate = !0;
              const N = I.distanceToTriangle(u, d, T);
              if (N < R && (E.copy(d), p && p.copy(T), R = N, S = b, m = w), N < n)
                return !0;
            }
          }
        }
      }
    }
  ), $t.releasePrimitive(I), $t.releasePrimitive(u), R === 1 / 0 ? null : (e.point ? e.point.copy(E) : e.point = E.clone(), e.distance = R, e.faceIndex = S, s && (s.point ? s.point.copy(p) : s.point = p.clone(), s.point.applyMatrix4(Ki), E.applyMatrix4(Ki), s.distance = E.sub(s.point).length(), s.faceIndex = m), e);
}
function Fa(c, i = null) {
  i && Array.isArray(i) && (i = new Set(i));
  const t = c.geometry, e = t.index ? t.index.array : null, s = t.attributes.position;
  let n, r, o, a, l = 0;
  const h = c._roots;
  for (let I = 0, u = h.length; I < u; I++)
    n = h[I], r = new Uint32Array(n), o = new Uint16Array(n), a = new Float32Array(n), f(0, l), l += n.byteLength;
  function f(I, u, d = !1) {
    const E = I * 2;
    if (o[E + 15] === Ys) {
      const p = r[I + 6], R = o[E + 14];
      let S = 1 / 0, m = 1 / 0, F = 1 / 0, O = -1 / 0, y = -1 / 0, w = -1 / 0;
      for (let L = p, b = p + R; L < b; L++) {
        const Y = 3 * c.resolveTriangleIndex(L);
        for (let N = 0; N < 3; N++) {
          let M = Y + N;
          M = e ? e[M] : M;
          const g = s.getX(M), v = s.getY(M), q = s.getZ(M);
          g < S && (S = g), g > O && (O = g), v < m && (m = v), v > y && (y = v), q < F && (F = q), q > w && (w = q);
        }
      }
      return a[I + 0] !== S || a[I + 1] !== m || a[I + 2] !== F || a[I + 3] !== O || a[I + 4] !== y || a[I + 5] !== w ? (a[I + 0] = S, a[I + 1] = m, a[I + 2] = F, a[I + 3] = O, a[I + 4] = y, a[I + 5] = w, !0) : !1;
    } else {
      const p = I + 8, R = r[I + 6], S = p + u, m = R + u;
      let F = d, O = !1, y = !1;
      i ? F || (O = i.has(S), y = i.has(m), F = !O && !y) : (O = !0, y = !0);
      const w = F || O, L = F || y;
      let b = !1;
      w && (b = f(p, u, F));
      let Y = !1;
      L && (Y = f(R, u, F));
      const N = b || Y;
      if (N)
        for (let M = 0; M < 3; M++) {
          const g = p + M, v = R + M, q = a[g], G = a[g + 3], et = a[v], W = a[v + 3];
          a[I + M] = q < et ? q : et, a[I + M + 3] = G > W ? G : W;
        }
      return N;
    }
  }
}
const lr = /* @__PURE__ */ new Q();
function Sa(c, i, t, e, s) {
  Ft.setBuffer(c._roots[i]), mn(0, c, t, e, s), Ft.clearBuffer();
}
function mn(c, i, t, e, s) {
  const { float32Array: n, uint16Array: r, uint32Array: o } = Ft, a = c * 2;
  if (zt(a, r)) {
    const h = kt(c, o), f = Zt(a, r);
    ua(i, t, e, h, f, s);
  } else {
    const h = jt(c);
    Me(h, n, e, lr) && mn(h, i, t, e, s);
    const f = qt(c, o);
    Me(f, n, e, lr) && mn(f, i, t, e, s);
  }
}
const hr = /* @__PURE__ */ new Q(), Oa = ["x", "y", "z"];
function Na(c, i, t, e) {
  Ft.setBuffer(c._roots[i]);
  const s = Rn(0, c, t, e);
  return Ft.clearBuffer(), s;
}
function Rn(c, i, t, e) {
  const { float32Array: s, uint16Array: n, uint32Array: r } = Ft;
  let o = c * 2;
  if (zt(o, n)) {
    const l = kt(c, r), h = Zt(o, n);
    return fa(i, t, e, l, h);
  } else {
    const l = eo(c, r), h = Oa[l], I = e.direction[h] >= 0;
    let u, d;
    I ? (u = jt(c), d = qt(c, r)) : (u = qt(c, r), d = jt(c));
    const T = Me(u, s, e, hr) ? Rn(u, i, t, e) : null;
    if (T) {
      const S = T.point[h];
      if (I ? S <= s[d + l] : (
        // min bounding data
        S >= s[d + l + 3]
      ))
        return T;
    }
    const R = Me(d, s, e, hr) ? Rn(d, i, t, e) : null;
    return T && R ? T.distance <= R.distance ? T : R : T || R || null;
  }
}
const Ji = /* @__PURE__ */ new ae(), Ze = /* @__PURE__ */ new ne(), je = /* @__PURE__ */ new ne(), Ti = /* @__PURE__ */ new pe(), ur = /* @__PURE__ */ new Bt(), ts = /* @__PURE__ */ new Bt();
function ya(c, i, t, e) {
  Ft.setBuffer(c._roots[i]);
  const s = gn(0, c, t, e);
  return Ft.clearBuffer(), s;
}
function gn(c, i, t, e, s = null) {
  const { float32Array: n, uint16Array: r, uint32Array: o } = Ft;
  let a = c * 2;
  if (s === null && (t.boundingBox || t.computeBoundingBox(), ur.set(t.boundingBox.min, t.boundingBox.max, e), s = ur), zt(a, r)) {
    const h = i.geometry, f = h.index, I = h.attributes.position, u = t.index, d = t.attributes.position, E = kt(c, o), T = Zt(a, r);
    if (Ti.copy(e).invert(), t.boundsTree)
      return Nt(c, n, ts), ts.matrix.copy(Ti), ts.needsUpdate = !0, t.boundsTree.shapecast({
        intersectsBounds: (R) => ts.intersectsBox(R),
        intersectsTriangle: (R) => {
          R.a.applyMatrix4(e), R.b.applyMatrix4(e), R.c.applyMatrix4(e), R.needsUpdate = !0;
          for (let S = E, m = T + E; S < m; S++)
            if (wt(je, 3 * i.resolveTriangleIndex(S), f, I), je.needsUpdate = !0, R.intersectsTriangle(je))
              return !0;
          return !1;
        }
      });
    for (let p = E, R = T + E; p < R; p++) {
      const S = i.resolveTriangleIndex(p);
      wt(Ze, 3 * S, f, I), Ze.a.applyMatrix4(Ti), Ze.b.applyMatrix4(Ti), Ze.c.applyMatrix4(Ti), Ze.needsUpdate = !0;
      for (let m = 0, F = u.count; m < F; m += 3)
        if (wt(je, m, u, d), je.needsUpdate = !0, Ze.intersectsTriangle(je))
          return !0;
    }
  } else {
    const h = c + 8, f = o[c + 6];
    return Nt(h, n, Ji), !!(s.intersectsBox(Ji) && gn(h, i, t, e, s) || (Nt(f, n, Ji), s.intersectsBox(Ji) && gn(f, i, t, e, s)));
  }
}
const es = /* @__PURE__ */ new pe(), Ks = /* @__PURE__ */ new Bt(), mi = /* @__PURE__ */ new Bt(), La = /* @__PURE__ */ new Q(), Pa = /* @__PURE__ */ new Q(), _a = /* @__PURE__ */ new Q(), wa = /* @__PURE__ */ new Q();
function Ma(c, i, t, e = {}, s = {}, n = 0, r = 1 / 0) {
  i.boundingBox || i.computeBoundingBox(), Ks.set(i.boundingBox.min, i.boundingBox.max, t), Ks.needsUpdate = !0;
  const o = c.geometry, a = o.attributes.position, l = o.index, h = i.attributes.position, f = i.index, I = $t.getPrimitive(), u = $t.getPrimitive();
  let d = La, E = Pa, T = null, p = null;
  s && (T = _a, p = wa);
  let R = 1 / 0, S = null, m = null;
  return es.copy(t).invert(), mi.matrix.copy(es), c.shapecast(
    {
      boundsTraverseOrder: (F) => Ks.distanceToBox(F),
      intersectsBounds: (F, O, y) => y < R && y < r ? (O && (mi.min.copy(F.min), mi.max.copy(F.max), mi.needsUpdate = !0), !0) : !1,
      intersectsRange: (F, O) => {
        if (i.boundsTree) {
          const y = i.boundsTree;
          return y.shapecast({
            boundsTraverseOrder: (w) => mi.distanceToBox(w),
            intersectsBounds: (w, L, b) => b < R && b < r,
            intersectsRange: (w, L) => {
              for (let b = w, Y = w + L; b < Y; b++) {
                const N = y.resolveTriangleIndex(b);
                wt(u, 3 * N, f, h), u.a.applyMatrix4(t), u.b.applyMatrix4(t), u.c.applyMatrix4(t), u.needsUpdate = !0;
                for (let M = F, g = F + O; M < g; M++) {
                  const v = c.resolveTriangleIndex(M);
                  wt(I, 3 * v, l, a), I.needsUpdate = !0;
                  const q = I.distanceToTriangle(u, d, T);
                  if (q < R && (E.copy(d), p && p.copy(T), R = q, S = M, m = b), q < n)
                    return !0;
                }
              }
            }
          });
        } else {
          const y = li(i);
          for (let w = 0, L = y; w < L; w++) {
            wt(u, 3 * w, f, h), u.a.applyMatrix4(t), u.b.applyMatrix4(t), u.c.applyMatrix4(t), u.needsUpdate = !0;
            for (let b = F, Y = F + O; b < Y; b++) {
              const N = c.resolveTriangleIndex(b);
              wt(I, 3 * N, l, a), I.needsUpdate = !0;
              const M = I.distanceToTriangle(u, d, T);
              if (M < R && (E.copy(d), p && p.copy(T), R = M, S = b, m = w), M < n)
                return !0;
            }
          }
        }
      }
    }
  ), $t.releasePrimitive(I), $t.releasePrimitive(u), R === 1 / 0 ? null : (e.point ? e.point.copy(E) : e.point = E.clone(), e.distance = R, e.faceIndex = S, s && (s.point ? s.point.copy(p) : s.point = p.clone(), s.point.applyMatrix4(es), E.applyMatrix4(es), s.distance = E.sub(s.point).length(), s.faceIndex = m), e);
}
function Da() {
  return typeof SharedArrayBuffer < "u";
}
const wi = new Ft.constructor(), ps = new Ft.constructor(), ye = new xn(() => new ae()), qe = new ae(), Qe = new ae(), Js = new ae(), tn = new ae();
let en = !1;
function ba(c, i, t, e) {
  if (en)
    throw new Error("MeshBVH: Recursive calls to bvhcast not supported.");
  en = !0;
  const s = c._roots, n = i._roots;
  let r, o = 0, a = 0;
  const l = new pe().copy(t).invert();
  for (let h = 0, f = s.length; h < f; h++) {
    wi.setBuffer(s[h]), a = 0;
    const I = ye.getPrimitive();
    Nt(0, wi.float32Array, I), I.applyMatrix4(l);
    for (let u = 0, d = n.length; u < d && (ps.setBuffer(n[h]), r = Kt(
      0,
      0,
      t,
      l,
      e,
      o,
      a,
      0,
      0,
      I
    ), ps.clearBuffer(), a += n[u].length, !r); u++)
      ;
    if (ye.releasePrimitive(I), wi.clearBuffer(), o += s[h].length, r)
      break;
  }
  return en = !1, r;
}
function Kt(c, i, t, e, s, n = 0, r = 0, o = 0, a = 0, l = null, h = !1) {
  let f, I;
  h ? (f = ps, I = wi) : (f = wi, I = ps);
  const u = f.float32Array, d = f.uint32Array, E = f.uint16Array, T = I.float32Array, p = I.uint32Array, R = I.uint16Array, S = c * 2, m = i * 2, F = zt(S, E), O = zt(m, R);
  let y = !1;
  if (O && F)
    h ? y = s(
      kt(i, p),
      Zt(i * 2, R),
      kt(c, d),
      Zt(c * 2, E),
      a,
      r + i,
      o,
      n + c
    ) : y = s(
      kt(c, d),
      Zt(c * 2, E),
      kt(i, p),
      Zt(i * 2, R),
      o,
      n + c,
      a,
      r + i
    );
  else if (O) {
    const w = ye.getPrimitive();
    Nt(i, T, w), w.applyMatrix4(t);
    const L = jt(c), b = qt(c, d);
    Nt(L, u, qe), Nt(b, u, Qe);
    const Y = w.intersectsBox(qe), N = w.intersectsBox(Qe);
    y = Y && Kt(
      i,
      L,
      e,
      t,
      s,
      r,
      n,
      a,
      o + 1,
      w,
      !h
    ) || N && Kt(
      i,
      b,
      e,
      t,
      s,
      r,
      n,
      a,
      o + 1,
      w,
      !h
    ), ye.releasePrimitive(w);
  } else {
    const w = jt(i), L = qt(i, p);
    Nt(w, T, Js), Nt(L, T, tn);
    const b = l.intersectsBox(Js), Y = l.intersectsBox(tn);
    if (b && Y)
      y = Kt(
        c,
        w,
        t,
        e,
        s,
        n,
        r,
        o,
        a + 1,
        l,
        h
      ) || Kt(
        c,
        L,
        t,
        e,
        s,
        n,
        r,
        o,
        a + 1,
        l,
        h
      );
    else if (b)
      if (F)
        y = Kt(
          c,
          w,
          t,
          e,
          s,
          n,
          r,
          o,
          a + 1,
          l,
          h
        );
      else {
        const N = ye.getPrimitive();
        N.copy(Js).applyMatrix4(t);
        const M = jt(c), g = qt(c, d);
        Nt(M, u, qe), Nt(g, u, Qe);
        const v = N.intersectsBox(qe), q = N.intersectsBox(Qe);
        y = v && Kt(
          w,
          M,
          e,
          t,
          s,
          r,
          n,
          a,
          o + 1,
          N,
          !h
        ) || q && Kt(
          w,
          g,
          e,
          t,
          s,
          r,
          n,
          a,
          o + 1,
          N,
          !h
        ), ye.releasePrimitive(N);
      }
    else if (Y)
      if (F)
        y = Kt(
          c,
          L,
          t,
          e,
          s,
          n,
          r,
          o,
          a + 1,
          l,
          h
        );
      else {
        const N = ye.getPrimitive();
        N.copy(tn).applyMatrix4(t);
        const M = jt(c), g = qt(c, d);
        Nt(M, u, qe), Nt(g, u, Qe);
        const v = N.intersectsBox(qe), q = N.intersectsBox(Qe);
        y = v && Kt(
          L,
          M,
          e,
          t,
          s,
          r,
          n,
          a,
          o + 1,
          N,
          !h
        ) || q && Kt(
          L,
          g,
          e,
          t,
          s,
          r,
          n,
          a,
          o + 1,
          N,
          !h
        ), ye.releasePrimitive(N);
      }
  }
  return y;
}
const is = /* @__PURE__ */ new Bt(), fr = /* @__PURE__ */ new ae();
class Bn {
  static serialize(i, t = {}) {
    t = {
      cloneBuffers: !0,
      ...t
    };
    const e = i.geometry, s = i._roots, n = i._indirectBuffer, r = e.getIndex();
    let o;
    return t.cloneBuffers ? o = {
      roots: s.map((a) => a.slice()),
      index: r.array.slice(),
      indirectBuffer: n ? n.slice() : null
    } : o = {
      roots: s,
      index: r.array,
      indirectBuffer: n
    }, o;
  }
  static deserialize(i, t, e = {}) {
    e = {
      setIndex: !0,
      indirect: !!i.indirectBuffer,
      ...e
    };
    const { index: s, roots: n, indirectBuffer: r } = i, o = new Bn(t, { ...e, [$s]: !0 });
    if (o._roots = n, o._indirectBuffer = r || null, e.setIndex) {
      const a = t.getIndex();
      if (a === null) {
        const l = new jr(i.index, 1, !1);
        t.setIndex(l);
      } else
        a.array !== s && (a.array.set(s), a.needsUpdate = !0);
    }
    return o;
  }
  get indirect() {
    return !!this._indirectBuffer;
  }
  constructor(i, t = {}) {
    if (i.isBufferGeometry) {
      if (i.index && i.index.isInterleavedBufferAttribute)
        throw new Error("MeshBVH: InterleavedBufferAttribute is not supported for the index attribute.");
    } else
      throw new Error("MeshBVH: Only BufferGeometries are supported.");
    if (t = Object.assign({
      strategy: Kr,
      maxDepth: 40,
      maxLeafTris: 10,
      verbose: !0,
      useSharedArrayBuffer: !1,
      setBoundingBox: !0,
      onProgress: null,
      indirect: !1,
      // undocumented options
      // Whether to skip generating the tree. Used for deserialization.
      [$s]: !1
    }, t), t.useSharedArrayBuffer && !Da())
      throw new Error("MeshBVH: SharedArrayBuffer is not available.");
    this.geometry = i, this._roots = null, this._indirectBuffer = null, t[$s] || (Qo(this, t), !i.boundingBox && t.setBoundingBox && (i.boundingBox = this.getBoundingBox(new ae())));
    const { _indirectBuffer: e } = this;
    this.resolveTriangleIndex = t.indirect ? (s) => e[s] : (s) => s;
  }
  refit(i = null) {
    return (this.indirect ? Fa : ha)(this, i);
  }
  traverse(i, t = 0) {
    const e = this._roots[t], s = new Uint32Array(e), n = new Uint16Array(e);
    r(0);
    function r(o, a = 0) {
      const l = o * 2, h = n[l + 15] === Ys;
      if (h) {
        const f = s[o + 6], I = n[l + 14];
        i(a, h, new Float32Array(e, o * 4, 6), f, I);
      } else {
        const f = o + Is / 4, I = s[o + 6], u = s[o + 7];
        i(a, h, new Float32Array(e, o * 4, 6), u) || (r(f, a + 1), r(I, a + 1));
      }
    }
  }
  /* Core Cast Functions */
  raycast(i, t = Zn) {
    const e = this._roots, s = this.geometry, n = [], r = t.isMaterial, o = Array.isArray(t), a = s.groups, l = r ? t.side : t, h = this.indirect ? Sa : da;
    for (let f = 0, I = e.length; f < I; f++) {
      const u = o ? t[a[f].materialIndex].side : l, d = n.length;
      if (h(this, f, u, i, n), o) {
        const E = a[f].materialIndex;
        for (let T = d, p = n.length; T < p; T++)
          n[T].face.materialIndex = E;
      }
    }
    return n;
  }
  raycastFirst(i, t = Zn) {
    const e = this._roots, s = this.geometry, n = t.isMaterial, r = Array.isArray(t);
    let o = null;
    const a = s.groups, l = n ? t.side : t, h = this.indirect ? Na : pa;
    for (let f = 0, I = e.length; f < I; f++) {
      const u = r ? t[a[f].materialIndex].side : l, d = h(this, f, u, i);
      d != null && (o == null || d.distance < o.distance) && (o = d, r && (d.face.materialIndex = a[f].materialIndex));
    }
    return o;
  }
  intersectsGeometry(i, t) {
    let e = !1;
    const s = this._roots, n = this.indirect ? ya : Ca;
    for (let r = 0, o = s.length; r < o && (e = n(this, r, i, t), !e); r++)
      ;
    return e;
  }
  shapecast(i) {
    const t = $t.getPrimitive(), e = this.indirect ? Ia : la;
    let {
      boundsTraverseOrder: s,
      intersectsBounds: n,
      intersectsRange: r,
      intersectsTriangle: o
    } = i;
    if (r && o) {
      const f = r;
      r = (I, u, d, E, T) => f(I, u, d, E, T) ? !0 : e(I, u, this, o, d, E, t);
    } else
      r || (o ? r = (f, I, u, d) => e(f, I, this, o, u, d, t) : r = (f, I, u) => u);
    let a = !1, l = 0;
    const h = this._roots;
    for (let f = 0, I = h.length; f < I; f++) {
      const u = h[f];
      if (a = sa(this, f, n, r, s, l), a)
        break;
      l += u.byteLength;
    }
    return $t.releasePrimitive(t), a;
  }
  bvhcast(i, t, e) {
    let {
      intersectsRanges: s,
      intersectsTriangles: n
    } = e;
    const r = $t.getPrimitive(), o = this.geometry.index, a = this.geometry.attributes.position, l = this.indirect ? (d) => {
      const E = this.resolveTriangleIndex(d);
      wt(r, E * 3, o, a);
    } : (d) => {
      wt(r, d * 3, o, a);
    }, h = $t.getPrimitive(), f = i.geometry.index, I = i.geometry.attributes.position, u = i.indirect ? (d) => {
      const E = i.resolveTriangleIndex(d);
      wt(h, E * 3, f, I);
    } : (d) => {
      wt(h, d * 3, f, I);
    };
    if (n) {
      const d = (E, T, p, R, S, m, F, O) => {
        for (let y = p, w = p + R; y < w; y++) {
          u(y), h.a.applyMatrix4(t), h.b.applyMatrix4(t), h.c.applyMatrix4(t), h.needsUpdate = !0;
          for (let L = E, b = E + T; L < b; L++)
            if (l(L), r.needsUpdate = !0, n(r, h, L, y, S, m, F, O))
              return !0;
        }
        return !1;
      };
      if (s) {
        const E = s;
        s = function(T, p, R, S, m, F, O, y) {
          return E(T, p, R, S, m, F, O, y) ? !0 : d(T, p, R, S, m, F, O, y);
        };
      } else
        s = d;
    }
    return ba(this, i, t, s);
  }
  /* Derived Cast Functions */
  intersectsBox(i, t) {
    return is.set(i.min, i.max, t), is.needsUpdate = !0, this.shapecast(
      {
        intersectsBounds: (e) => is.intersectsBox(e),
        intersectsTriangle: (e) => is.intersectsTriangle(e)
      }
    );
  }
  intersectsSphere(i) {
    return this.shapecast(
      {
        intersectsBounds: (t) => i.intersectsBox(t),
        intersectsTriangle: (t) => t.intersectsSphere(i)
      }
    );
  }
  closestPointToGeometry(i, t, e = {}, s = {}, n = 0, r = 1 / 0) {
    return (this.indirect ? Ma : Aa)(
      this,
      i,
      t,
      e,
      s,
      n,
      r
    );
  }
  closestPointToPoint(i, t = {}, e = 0, s = 1 / 0) {
    return na(
      this,
      i,
      t,
      e,
      s
    );
  }
  getBoundingBox(i) {
    return i.makeEmpty(), this._roots.forEach((e) => {
      Nt(0, new Float32Array(e), fr), i.union(fr);
    }), i;
  }
}
function Ir(c, i, t) {
  return c === null || (c.point.applyMatrix4(i.matrixWorld), c.distance = c.point.distanceTo(t.ray.origin), c.object = i, c.distance < t.near || c.distance > t.far) ? null : c;
}
const sn = /* @__PURE__ */ new yo(), dr = /* @__PURE__ */ new pe(), va = rt.prototype.raycast;
function Ua(c, i) {
  if (this.geometry.boundsTree) {
    if (this.material === void 0)
      return;
    dr.copy(this.matrixWorld).invert(), sn.copy(c.ray).applyMatrix4(dr);
    const t = this.geometry.boundsTree;
    if (c.firstHitOnly === !0) {
      const e = Ir(t.raycastFirst(sn, this.material), this, c);
      e && i.push(e);
    } else {
      const e = t.raycast(sn, this.material);
      for (let s = 0, n = e.length; s < n; s++) {
        const r = Ir(e[s], this, c);
        r && i.push(r);
      }
    }
  } else
    va.call(this, c, i);
}
function xa(c) {
  return this.boundsTree = new Bn(this, c), this.boundsTree;
}
function Ba() {
  this.boundsTree = null;
}
class j {
  constructor() {
    /** Triggers all the callbacks assigned to this event. */
    C(this, "trigger", (i) => {
      const t = this.handlers.slice(0);
      for (const e of t)
        e(i);
    });
    C(this, "handlers", []);
  }
  /**
   * Add a callback to this event instance.
   * @param handler - the callback to be added to this event.
   */
  add(i) {
    this.handlers.push(i);
  }
  /**
   * Removes a callback from this event instance.
   * @param handler - the callback to be removed from this event.
   */
  remove(i) {
    this.handlers = this.handlers.filter((t) => t !== i);
  }
  /** Gets rid of all the suscribed events. */
  reset() {
    this.handlers.length = 0;
  }
}
class _e {
  constructor() {
    /** Triggers all the callbacks assigned to this event. */
    C(this, "trigger", async (i) => {
      const t = this.handlers.slice(0);
      for (const e of t)
        await e(i);
    });
    C(this, "handlers", []);
  }
  /**
   * Add a callback to this event instance.
   * @param handler - the callback to be added to this event.
   */
  add(i) {
    this.handlers.push(i);
  }
  /**
   * Removes a callback from this event instance.
   * @param handler - the callback to be removed from this event.
   */
  remove(i) {
    this.handlers = this.handlers.filter((t) => t !== i);
  }
  /** Gets rid of all the suscribed events. */
  reset() {
    this.handlers.length = 0;
  }
}
class Vn {
  constructor(i) {
    /** Whether is component is {@link Disposable}. */
    C(this, "isDisposeable", () => "dispose" in this && "onDisposed" in this);
    /** Whether is component is {@link Resizeable}. */
    C(this, "isResizeable", () => "resize" in this && "getSize" in this);
    /** Whether is component is {@link Updateable}. */
    C(this, "isUpdateable", () => "onAfterUpdate" in this && "onBeforeUpdate" in this && "update" in this);
    /** Whether is component is {@link Hideable}. */
    C(this, "isHideable", () => "visible" in this);
    /** Whether is component is {@link Configurable}. */
    C(this, "isConfigurable", () => "setup" in this && "config" in this && "onSetup" in this);
    this.components = i;
  }
}
class At extends Vn {
}
class Yn extends Vn {
  constructor(t) {
    super(t);
    C(this, "worlds", /* @__PURE__ */ new Map());
    /**
     * Event that is triggered when a world is added or removed from the `worlds` map.
     * The event payload contains the world instance and the action ("added" or "removed").
     */
    C(this, "onWorldChanged", new j());
    /**
     * The current world this item is associated with. It can be null if no world is currently active.
     */
    C(this, "currentWorld", null);
    this.onWorldChanged.add(({ world: e, action: s }) => {
      s === "removed" && this.worlds.delete(e.uuid);
    });
  }
}
class Va extends Yn {
  constructor() {
    super(...arguments);
    /**
     * Checks whether the instance is {@link CameraControllable}.
     *
     * @returns True if the instance is controllable, false otherwise.
     */
    C(this, "hasCameraControls", () => "controls" in this);
  }
}
class Ya extends Yn {
  constructor() {
    super(...arguments);
    /** {@link Updateable.onBeforeUpdate} */
    C(this, "onAfterUpdate", new j());
    /** {@link Updateable.onAfterUpdate} */
    C(this, "onBeforeUpdate", new j());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /** {@link Resizeable.onResize} */
    C(this, "onResize", new j());
    /**
     * Event that fires when there has been a change to the list of clipping
     * planes used by the active renderer.
     */
    C(this, "onClippingPlanesUpdated", new j());
    /**
     * The list of [clipping planes](https://threejs.org/docs/#api/en/renderers/WebGLRenderer.clippingPlanes) used by this instance of the renderer.
     */
    C(this, "clippingPlanes", []);
  }
  /**
   * Updates the clipping planes and triggers the `onClippingPlanesUpdated` event.
   *
   * @remarks
   * This method is typically called when there is a change to the list of clipping planes
   * used by the active renderer.
   */
  updateClippingPlanes() {
    this.onClippingPlanesUpdated.trigger();
  }
  /**
   * Sets or removes a clipping plane from the renderer.
   *
   * @param active - A boolean indicating whether the clipping plane should be active or not.
   * @param plane - The clipping plane to be added or removed.
   * @param isLocal - An optional boolean indicating whether the clipping plane is local to the object. If not provided, it defaults to `false`.
   *
   * @remarks
   * This method adds or removes a clipping plane from the `clippingPlanes` array.
   * If `active` is `true` and the plane is not already in the array, it is added.
   * If `active` is `false` and the plane is in the array, it is removed.
   * The `three.clippingPlanes` property is then updated to reflect the current state of the `clippingPlanes` array,
   * excluding any planes marked as local.
   */
  setPlane(t, e, s) {
    e.isLocal = s;
    const n = this.clippingPlanes.indexOf(e);
    t && n === -1 ? this.clippingPlanes.push(e) : !t && n > -1 && this.clippingPlanes.splice(n, 1), this.three.clippingPlanes = this.clippingPlanes.filter(
      (r) => !r.isLocal
    );
  }
}
const Mi = class Mi extends At {
  constructor(t) {
    super(t);
    C(this, "_disposedComponents", /* @__PURE__ */ new Set());
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    t.add(Mi.uuid, this);
  }
  // TODO: Remove this?
  /**
   * Return the UUIDs of all disposed components.
   */
  get() {
    return this._disposedComponents;
  }
  /**
   * Removes a mesh, its geometry and its materials from memory. If you are
   * using any of these in other parts of the application, make sure that you
   * remove them from the mesh before disposing it.
   *
   * @param object - the [object](https://threejs.org/docs/#api/en/core/Object3D)
   * to remove.
   *
   * @param materials - whether to dispose the materials of the mesh.
   *
   * @param recursive - whether to recursively dispose the children of the mesh.
   */
  destroy(t, e = !0, s = !0) {
    t.removeFromParent();
    const n = t;
    n.dispose && n.dispose(), this.disposeGeometryAndMaterials(t, e), s && n.children && n.children.length && this.disposeChildren(n), t.children.length = 0;
  }
  /**
   * Disposes a geometry from memory.
   *
   * @param geometry - the
   * [geometry](https://threejs.org/docs/#api/en/core/BufferGeometry)
   * to remove.
   */
  disposeGeometry(t) {
    t.boundsTree && t.disposeBoundsTree && t.disposeBoundsTree(), t.dispose();
  }
  disposeGeometryAndMaterials(t, e) {
    const s = t;
    s.geometry && this.disposeGeometry(s.geometry), e && s.material && Mi.disposeMaterial(s), s.material = [], s.geometry = null;
  }
  disposeChildren(t) {
    for (const e of t.children)
      this.destroy(e);
  }
  static disposeMaterial(t) {
    if (t.material)
      if (Array.isArray(t.material))
        for (const e of t.material)
          e.dispose();
      else
        t.material.dispose();
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(Mi, "uuid", "76e9cd8e-ad8f-4753-9ef6-cbc60f7247fe");
let De = Mi;
class Ga extends Yn {
  constructor(t) {
    super(t);
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /** The set of directional lights managed by this scene component. */
    C(this, "directionalLights", /* @__PURE__ */ new Map());
    /** The set of ambient lights managed by this scene component. */
    C(this, "ambientLights", /* @__PURE__ */ new Map());
  }
  /** {@link Disposable.dispose} */
  dispose() {
    const t = this.components.get(De);
    for (const e of this.three.children) {
      const s = e;
      s.geometry && t.destroy(s);
    }
    this.deleteAllLights(), this.three.children = [], this.onDisposed.trigger(), this.onDisposed.reset();
  }
  deleteAllLights() {
    for (const [, t] of this.directionalLights)
      t.removeFromParent(), t.target.removeFromParent(), t.dispose();
    this.directionalLights.clear();
    for (const [, t] of this.ambientLights)
      t.removeFromParent(), t.dispose();
    this.ambientLights.clear();
  }
}
class we extends Set {
  /**
   * Constructs a new instance of the DataSet class.
   *
   * @param iterable - An optional iterable object to initialize the set with.
   */
  constructor(t) {
    super(t);
    /**
     * An event that is triggered when a new item is added to the set.
     */
    C(this, "onItemAdded", new j());
    /**
     * An event that is triggered when an item is deleted from the set.
     */
    C(this, "onItemDeleted", new j());
    /**
     * An event that is triggered when the set is cleared.
     */
    C(this, "onCleared", new j());
    /**
     * A function that acts as a guard for adding items to the set.
     * It determines whether a given value should be allowed to be added to the set.
     *
     * @param value - The value to be checked against the guard.
     * @returns A boolean indicating whether the value should be allowed to be added to the set.
     *          By default, this function always returns true, allowing all values to be added.
     *          You can override this behavior by providing a custom implementation.
     */
    C(this, "guard", () => !0);
  }
  /**
   * Clears the set and triggers the onCleared event.
   */
  clear() {
    super.clear(), this.onCleared.trigger();
  }
  /**
   * Adds one or multiple values to the set and triggers the onItemAdded event per each.
   *
   * @param value - The value to add to the set.
   * @returns - The set instance.
   */
  add(...t) {
    for (const e of t)
      this.has(e) || !this.guard(e) || (super.add(e), this.onItemAdded || (this.onItemAdded = new j()), this.onItemAdded.trigger(e));
    return this;
  }
  /**
   * Deletes a value from the set and triggers the onItemDeleted event.
   *
   * @param value - The value to delete from the set.
   * @returns - True if the value was successfully deleted, false otherwise.
   */
  delete(t) {
    const e = super.delete(t);
    return e && this.onItemDeleted.trigger(), e;
  }
  /**
   * Clears the set and resets the onItemAdded, onItemDeleted, and onCleared events.
   */
  dispose() {
    this.clear(), this.onItemAdded.reset(), this.onItemDeleted.reset(), this.onCleared.reset();
  }
}
class re extends Map {
  /**
   * Constructs a new DataMap instance.
   *
   * @param iterable - An iterable object containing key-value pairs to populate the map.
   */
  constructor(t) {
    super(t);
    /**
     * An event triggered when a new item is set in the map.
     */
    C(this, "onItemSet", new j());
    /**
     * An event triggered when an existing item in the map is updated.
     */
    C(this, "onItemUpdated", new j());
    /**
     * An event triggered when an item is deleted from the map.
     */
    C(this, "onItemDeleted", new j());
    /**
     * An event triggered when the map is cleared.
     */
    C(this, "onCleared", new j());
    /**
     * A function that acts as a guard for adding items to the set.
     * It determines whether a given value should be allowed to be added to the set.
     *
     * @param key - The key of the entry to be checked against the guard.
     * @param value - The value of the entry to be checked against the guard.
     * @returns A boolean indicating whether the value should be allowed to be added to the set.
     *          By default, this function always returns true, allowing all values to be added.
     *          You can override this behavior by providing a custom implementation.
     */
    C(this, "guard", () => !0);
  }
  /**
   * Clears the map and triggers the onCleared event.
   */
  clear() {
    super.clear(), this.onCleared.trigger();
  }
  /**
   * Sets the value for the specified key in the map.
   * If the item is new, then onItemSet is triggered.
   * If the item is already in the map, then onItemUpdated is triggered.
   *
   * @param key - The key of the item to set.
   * @param value - The value of the item to set.
   * @returns The DataMap instance.
   */
  set(t, e) {
    const s = this.has(t);
    if (!(this.guard ?? (() => !0))(t, e))
      return this;
    const o = super.set(t, e);
    return s ? (this.onItemUpdated || (this.onItemUpdated = new j()), this.onItemUpdated.trigger({ key: t, value: e })) : (this.onItemSet || (this.onItemSet = new j()), this.onItemSet.trigger({ key: t, value: e })), o;
  }
  /**
   * Deletes the specified key from the map and triggers the onItemDeleted event if the key was found.
   *
   * @param key - The key of the item to delete.
   * @returns True if the key was found and deleted; otherwise, false.
   */
  delete(t) {
    const e = super.delete(t);
    return e && this.onItemDeleted.trigger(t), e;
  }
  /**
   * Clears the map and resets the events.
   */
  dispose() {
    this.clear(), this.onItemSet.reset(), this.onItemDeleted.reset(), this.onCleared.reset();
  }
}
class ph extends At {
}
const ds = 0, za = 1, ka = new Q(), Er = new Ee(), nn = new vn(), pr = new Q(), ss = new ii();
class Ha {
  constructor() {
    this.tolerance = -1, this.faces = [], this.newFaces = [], this.assigned = new Cr(), this.unassigned = new Cr(), this.vertices = [];
  }
  setFromPoints(i) {
    if (i.length >= 4) {
      this.makeEmpty();
      for (let t = 0, e = i.length; t < e; t++)
        this.vertices.push(new Wa(i[t]));
      this.compute();
    }
    return this;
  }
  setFromObject(i) {
    const t = [];
    return i.updateMatrixWorld(!0), i.traverse(function(e) {
      const s = e.geometry;
      if (s !== void 0) {
        const n = s.attributes.position;
        if (n !== void 0)
          for (let r = 0, o = n.count; r < o; r++) {
            const a = new Q();
            a.fromBufferAttribute(n, r).applyMatrix4(e.matrixWorld), t.push(a);
          }
      }
    }), this.setFromPoints(t);
  }
  containsPoint(i) {
    const t = this.faces;
    for (let e = 0, s = t.length; e < s; e++)
      if (t[e].distanceToPoint(i) > this.tolerance)
        return !1;
    return !0;
  }
  intersectRay(i, t) {
    const e = this.faces;
    let s = -1 / 0, n = 1 / 0;
    for (let r = 0, o = e.length; r < o; r++) {
      const a = e[r], l = a.distanceToPoint(i.origin), h = a.normal.dot(i.direction);
      if (l > 0 && h >= 0)
        return null;
      const f = h !== 0 ? -l / h : 0;
      if (!(f <= 0) && (h > 0 ? n = Math.min(f, n) : s = Math.max(f, s), s > n))
        return null;
    }
    return s !== -1 / 0 ? i.at(s, t) : i.at(n, t), t;
  }
  intersectsRay(i) {
    return this.intersectRay(i, ka) !== null;
  }
  makeEmpty() {
    return this.faces = [], this.vertices = [], this;
  }
  // Adds a vertex to the 'assigned' list of vertices and assigns it to the given face
  addVertexToFace(i, t) {
    return i.face = t, t.outside === null ? this.assigned.append(i) : this.assigned.insertBefore(t.outside, i), t.outside = i, this;
  }
  // Removes a vertex from the 'assigned' list of vertices and from the given face
  removeVertexFromFace(i, t) {
    return i === t.outside && (i.next !== null && i.next.face === t ? t.outside = i.next : t.outside = null), this.assigned.remove(i), this;
  }
  // Removes all the visible vertices that a given face is able to see which are stored in the 'assigned' vertex list
  removeAllVerticesFromFace(i) {
    if (i.outside !== null) {
      const t = i.outside;
      let e = i.outside;
      for (; e.next !== null && e.next.face === i; )
        e = e.next;
      return this.assigned.removeSubList(t, e), t.prev = e.next = null, i.outside = null, t;
    }
  }
  // Removes all the visible vertices that 'face' is able to see
  deleteFaceVertices(i, t) {
    const e = this.removeAllVerticesFromFace(i);
    if (e !== void 0)
      if (t === void 0)
        this.unassigned.appendChain(e);
      else {
        let s = e;
        do {
          const n = s.next;
          t.distanceToPoint(s.point) > this.tolerance ? this.addVertexToFace(s, t) : this.unassigned.append(s), s = n;
        } while (s !== null);
      }
    return this;
  }
  // Reassigns as many vertices as possible from the unassigned list to the new faces
  resolveUnassignedPoints(i) {
    if (this.unassigned.isEmpty() === !1) {
      let t = this.unassigned.first();
      do {
        const e = t.next;
        let s = this.tolerance, n = null;
        for (let r = 0; r < i.length; r++) {
          const o = i[r];
          if (o.mark === ds) {
            const a = o.distanceToPoint(t.point);
            if (a > s && (s = a, n = o), s > 1e3 * this.tolerance)
              break;
          }
        }
        n !== null && this.addVertexToFace(t, n), t = e;
      } while (t !== null);
    }
    return this;
  }
  // Computes the extremes of a simplex which will be the initial hull
  computeExtremes() {
    const i = new Q(), t = new Q(), e = [], s = [];
    for (let n = 0; n < 3; n++)
      e[n] = s[n] = this.vertices[0];
    i.copy(this.vertices[0].point), t.copy(this.vertices[0].point);
    for (let n = 0, r = this.vertices.length; n < r; n++) {
      const o = this.vertices[n], a = o.point;
      for (let l = 0; l < 3; l++)
        a.getComponent(l) < i.getComponent(l) && (i.setComponent(l, a.getComponent(l)), e[l] = o);
      for (let l = 0; l < 3; l++)
        a.getComponent(l) > t.getComponent(l) && (t.setComponent(l, a.getComponent(l)), s[l] = o);
    }
    return this.tolerance = 3 * Number.EPSILON * (Math.max(Math.abs(i.x), Math.abs(t.x)) + Math.max(Math.abs(i.y), Math.abs(t.y)) + Math.max(Math.abs(i.z), Math.abs(t.z))), { min: e, max: s };
  }
  // Computes the initial simplex assigning to its faces all the points
  // that are candidates to form part of the hull
  computeInitialHull() {
    const i = this.vertices, t = this.computeExtremes(), e = t.min, s = t.max;
    let n = 0, r = 0;
    for (let I = 0; I < 3; I++) {
      const u = s[I].point.getComponent(I) - e[I].point.getComponent(I);
      u > n && (n = u, r = I);
    }
    const o = e[r], a = s[r];
    let l, h;
    n = 0, Er.set(o.point, a.point);
    for (let I = 0, u = this.vertices.length; I < u; I++) {
      const d = i[I];
      if (d !== o && d !== a) {
        Er.closestPointToPoint(d.point, !0, pr);
        const E = pr.distanceToSquared(d.point);
        E > n && (n = E, l = d);
      }
    }
    n = -1, nn.setFromCoplanarPoints(o.point, a.point, l.point);
    for (let I = 0, u = this.vertices.length; I < u; I++) {
      const d = i[I];
      if (d !== o && d !== a && d !== l) {
        const E = Math.abs(nn.distanceToPoint(d.point));
        E > n && (n = E, h = d);
      }
    }
    const f = [];
    if (nn.distanceToPoint(h.point) < 0) {
      f.push(
        te.create(o, a, l),
        te.create(h, a, o),
        te.create(h, l, a),
        te.create(h, o, l)
      );
      for (let I = 0; I < 3; I++) {
        const u = (I + 1) % 3;
        f[I + 1].getEdge(2).setTwin(f[0].getEdge(u)), f[I + 1].getEdge(1).setTwin(f[u + 1].getEdge(0));
      }
    } else {
      f.push(
        te.create(o, l, a),
        te.create(h, o, a),
        te.create(h, a, l),
        te.create(h, l, o)
      );
      for (let I = 0; I < 3; I++) {
        const u = (I + 1) % 3;
        f[I + 1].getEdge(2).setTwin(f[0].getEdge((3 - I) % 3)), f[I + 1].getEdge(0).setTwin(f[u + 1].getEdge(1));
      }
    }
    for (let I = 0; I < 4; I++)
      this.faces.push(f[I]);
    for (let I = 0, u = i.length; I < u; I++) {
      const d = i[I];
      if (d !== o && d !== a && d !== l && d !== h) {
        n = this.tolerance;
        let E = null;
        for (let T = 0; T < 4; T++) {
          const p = this.faces[T].distanceToPoint(d.point);
          p > n && (n = p, E = this.faces[T]);
        }
        E !== null && this.addVertexToFace(d, E);
      }
    }
    return this;
  }
  // Removes inactive faces
  reindexFaces() {
    const i = [];
    for (let t = 0; t < this.faces.length; t++) {
      const e = this.faces[t];
      e.mark === ds && i.push(e);
    }
    return this.faces = i, this;
  }
  // Finds the next vertex to create faces with the current hull
  nextVertexToAdd() {
    if (this.assigned.isEmpty() === !1) {
      let i, t = 0;
      const e = this.assigned.first().face;
      let s = e.outside;
      do {
        const n = e.distanceToPoint(s.point);
        n > t && (t = n, i = s), s = s.next;
      } while (s !== null && s.face === e);
      return i;
    }
  }
  // Computes a chain of half edges in CCW order called the 'horizon'.
  // For an edge to be part of the horizon it must join a face that can see
  // 'eyePoint' and a face that cannot see 'eyePoint'.
  computeHorizon(i, t, e, s) {
    this.deleteFaceVertices(e), e.mark = za;
    let n;
    t === null ? n = t = e.getEdge(0) : n = t.next;
    do {
      const r = n.twin, o = r.face;
      o.mark === ds && (o.distanceToPoint(i) > this.tolerance ? this.computeHorizon(i, r, o, s) : s.push(n)), n = n.next;
    } while (n !== t);
    return this;
  }
  // Creates a face with the vertices 'eyeVertex.point', 'horizonEdge.tail' and 'horizonEdge.head' in CCW order
  addAdjoiningFace(i, t) {
    const e = te.create(i, t.tail(), t.head());
    return this.faces.push(e), e.getEdge(-1).setTwin(t.twin), e.getEdge(0);
  }
  //  Adds 'horizon.length' faces to the hull, each face will be linked with the
  //  horizon opposite face and the face on the left/right
  addNewFaces(i, t) {
    this.newFaces = [];
    let e = null, s = null;
    for (let n = 0; n < t.length; n++) {
      const r = t[n], o = this.addAdjoiningFace(i, r);
      e === null ? e = o : o.next.setTwin(s), this.newFaces.push(o.face), s = o;
    }
    return e.next.setTwin(s), this;
  }
  // Adds a vertex to the hull
  addVertexToHull(i) {
    const t = [];
    return this.unassigned.clear(), this.removeVertexFromFace(i, i.face), this.computeHorizon(i.point, null, i.face, t), this.addNewFaces(i, t), this.resolveUnassignedPoints(this.newFaces), this;
  }
  cleanup() {
    return this.assigned.clear(), this.unassigned.clear(), this.newFaces = [], this;
  }
  compute() {
    let i;
    for (this.computeInitialHull(); (i = this.nextVertexToAdd()) !== void 0; )
      this.addVertexToHull(i);
    return this.reindexFaces(), this.cleanup(), this;
  }
}
class te {
  constructor() {
    this.normal = new Q(), this.midpoint = new Q(), this.area = 0, this.constant = 0, this.outside = null, this.mark = ds, this.edge = null;
  }
  static create(i, t, e) {
    const s = new te(), n = new rn(i, s), r = new rn(t, s), o = new rn(e, s);
    return n.next = o.prev = r, r.next = n.prev = o, o.next = r.prev = n, s.edge = n, s.compute();
  }
  getEdge(i) {
    let t = this.edge;
    for (; i > 0; )
      t = t.next, i--;
    for (; i < 0; )
      t = t.prev, i++;
    return t;
  }
  compute() {
    const i = this.edge.tail(), t = this.edge.head(), e = this.edge.next.head();
    return ss.set(i.point, t.point, e.point), ss.getNormal(this.normal), ss.getMidpoint(this.midpoint), this.area = ss.getArea(), this.constant = this.normal.dot(this.midpoint), this;
  }
  distanceToPoint(i) {
    return this.normal.dot(i) - this.constant;
  }
}
class rn {
  constructor(i, t) {
    this.vertex = i, this.prev = null, this.next = null, this.twin = null, this.face = t;
  }
  head() {
    return this.vertex;
  }
  tail() {
    return this.prev ? this.prev.vertex : null;
  }
  length() {
    const i = this.head(), t = this.tail();
    return t !== null ? t.point.distanceTo(i.point) : -1;
  }
  lengthSquared() {
    const i = this.head(), t = this.tail();
    return t !== null ? t.point.distanceToSquared(i.point) : -1;
  }
  setTwin(i) {
    return this.twin = i, i.twin = this, this;
  }
}
class Wa {
  constructor(i) {
    this.point = i, this.prev = null, this.next = null, this.face = null;
  }
}
class Cr {
  constructor() {
    this.head = null, this.tail = null;
  }
  first() {
    return this.head;
  }
  last() {
    return this.tail;
  }
  clear() {
    return this.head = this.tail = null, this;
  }
  // Inserts a vertex before the target vertex
  insertBefore(i, t) {
    return t.prev = i.prev, t.next = i, t.prev === null ? this.head = t : t.prev.next = t, i.prev = t, this;
  }
  // Inserts a vertex after the target vertex
  insertAfter(i, t) {
    return t.prev = i, t.next = i.next, t.next === null ? this.tail = t : t.next.prev = t, i.next = t, this;
  }
  // Appends a vertex to the end of the linked list
  append(i) {
    return this.head === null ? this.head = i : this.tail.next = i, i.prev = this.tail, i.next = null, this.tail = i, this;
  }
  // Appends a chain of vertices where 'vertex' is the head.
  appendChain(i) {
    for (this.head === null ? this.head = i : this.tail.next = i, i.prev = this.tail; i.next !== null; )
      i = i.next;
    return this.tail = i, this;
  }
  // Removes a vertex from the linked list
  remove(i) {
    return i.prev === null ? this.head = i.next : i.prev.next = i.next, i.next === null ? this.tail = i.prev : i.next.prev = i.prev, this;
  }
  // Removes a list of vertices whose 'head' is 'a' and whose 'tail' is b
  removeSubList(i, t) {
    return i.prev === null ? this.head = t.next : i.prev.next = t.next, t.next === null ? this.tail = i.prev : t.next.prev = i.prev, this;
  }
  isEmpty() {
    return this.head === null;
  }
}
const An = [2, 2, 1], Fn = [1, 0, 0];
function fe(c, i) {
  return c * 3 + i;
}
function Xa(c) {
  const i = c.elements;
  let t = 0;
  for (let e = 0; e < 9; e++)
    t += i[e] * i[e];
  return Math.sqrt(t);
}
function $a(c) {
  const i = c.elements;
  let t = 0;
  for (let e = 0; e < 3; e++) {
    const s = i[fe(An[e], Fn[e])];
    t += 2 * s * s;
  }
  return Math.sqrt(t);
}
function Za(c, i) {
  let t = 0, e = 1;
  const s = c.elements;
  for (let l = 0; l < 3; l++) {
    const h = Math.abs(s[fe(An[l], Fn[l])]);
    h > t && (t = h, e = l);
  }
  let n = 1, r = 0;
  const o = Fn[e], a = An[e];
  if (Math.abs(s[fe(a, o)]) > Number.EPSILON) {
    const l = s[fe(a, a)], h = s[fe(o, o)], f = s[fe(a, o)], I = (l - h) / 2 / f;
    let u;
    I < 0 ? u = -1 / (-I + Math.sqrt(1 + I * I)) : u = 1 / (I + Math.sqrt(1 + I * I)), n = 1 / Math.sqrt(1 + u * u), r = u * n;
  }
  return i.identity(), i.elements[fe(o, o)] = n, i.elements[fe(a, a)] = n, i.elements[fe(a, o)] = r, i.elements[fe(o, a)] = -r, i;
}
function ja(c, i) {
  let t = 0, e = 0;
  const s = 10;
  i.unitary.identity(), i.diagonal.copy(c);
  const n = i.unitary, r = i.diagonal, o = new D.Matrix3(), a = new D.Matrix3(), l = Number.EPSILON * Xa(r);
  for (; e < s && $a(r) > l; )
    Za(r, o), a.copy(o).transpose(), r.multiply(o), r.premultiply(a), n.multiply(o), ++t > 2 && (e++, t = 0);
  return i;
}
function qa(c) {
  const i = [];
  for (let at = 0; at < c.length - 2; at += 3) {
    const It = c[at], ft = c[at + 1], Ct = c[at + 2];
    i.push(new D.Vector3(It, ft, Ct));
  }
  const t = new Ha();
  t.setFromPoints(i);
  const e = {
    unitary: new D.Matrix3(),
    diagonal: new D.Matrix3()
  }, s = t.faces, n = [], r = [];
  for (let at = 0, It = s.length; at < It; at++) {
    const ft = s[at];
    let Ct = ft.edge;
    n.length = 0;
    do
      n.push(Ct), Ct = Ct.next;
    while (Ct !== ft.edge);
    const Ht = n.length - 2;
    for (let Dt = 1, A = Ht; Dt <= A; Dt++) {
      const X = n[0].vertex, z = n[Dt + 0].vertex, _ = n[Dt + 1].vertex;
      r.push(X.point.x, X.point.y, X.point.z), r.push(z.point.x, z.point.y, z.point.z), r.push(_.point.x, _.point.y, _.point.z);
    }
  }
  const o = new D.Vector3(), a = new D.Vector3(), l = new D.Vector3(), h = new D.Vector3(), f = new D.Vector3(), I = new D.Vector3(), u = new D.Vector3(), d = new D.Vector3();
  let E = 0, T = 0, p = 0, R = 0, S = 0, m = 0, F = 0;
  for (let at = 0, It = r.length; at < It; at += 9) {
    o.fromArray(r, at), a.fromArray(r, at + 3), l.fromArray(r, at + 6), u.set(0, 0, 0), u.add(o).add(a).add(l).divideScalar(3), h.subVectors(a, o), f.subVectors(l, o);
    const ft = I.crossVectors(h, f).length() / 2;
    d.add(I.copy(u).multiplyScalar(ft)), E += ft, T += (9 * u.x * u.x + o.x * o.x + a.x * a.x + l.x * l.x) * (ft / 12), p += (9 * u.x * u.y + o.x * o.y + a.x * a.y + l.x * l.y) * (ft / 12), R += (9 * u.x * u.z + o.x * o.z + a.x * a.z + l.x * l.z) * (ft / 12), S += (9 * u.y * u.y + o.y * o.y + a.y * a.y + l.y * l.y) * (ft / 12), m += (9 * u.y * u.z + o.y * o.z + a.y * a.z + l.y * l.z) * (ft / 12), F += (9 * u.z * u.z + o.z * o.z + a.z * a.z + l.z * l.z) * (ft / 12);
  }
  d.divideScalar(E), T /= E, p /= E, R /= E, S /= E, m /= E, F /= E, T -= d.x * d.x, p -= d.x * d.y, R -= d.x * d.z, S -= d.y * d.y, m -= d.y * d.z, F -= d.z * d.z;
  const O = new D.Matrix3();
  O.elements[0] = T, O.elements[1] = p, O.elements[2] = R, O.elements[3] = p, O.elements[4] = S, O.elements[5] = m, O.elements[6] = R, O.elements[7] = m, O.elements[8] = F, ja(O, e);
  const y = e.unitary, w = new D.Vector3(), L = new D.Vector3(), b = new D.Vector3();
  y.extractBasis(w, L, b);
  let Y = -1 / 0, N = -1 / 0, M = -1 / 0, g = 1 / 0, v = 1 / 0, q = 1 / 0;
  for (let at = 0, It = i.length; at < It; at++) {
    const ft = i[at];
    Y = Math.max(w.dot(ft), Y), N = Math.max(L.dot(ft), N), M = Math.max(b.dot(ft), M), g = Math.min(w.dot(ft), g), v = Math.min(L.dot(ft), v), q = Math.min(b.dot(ft), q);
  }
  w.multiplyScalar(0.5 * (g + Y)), L.multiplyScalar(0.5 * (v + N)), b.multiplyScalar(0.5 * (q + M));
  const G = new D.Vector3(), et = new D.Vector3(), W = new D.Matrix3();
  G.add(w).add(L).add(b), et.x = Y - g, et.y = N - v, et.z = M - q, et.multiplyScalar(0.5), W.copy(y);
  const { x: nt, y: V, z: x } = et, ot = new D.Matrix4();
  ot.makeScale(nt * 2, V * 2, x * 2);
  const it = new D.Matrix4();
  it.makeTranslation(-nt, -V, -x);
  const tt = new D.Matrix4();
  tt.makeTranslation(G.x, G.y, G.z);
  const St = new D.Matrix4();
  St.setFromMatrix3(W);
  const yt = new D.Matrix4();
  return yt.multiply(tt), yt.multiply(St), yt.multiply(it), yt.multiply(ot), { center: G, halfSizes: et, rotation: W, transformation: yt };
}
function Ch(c, i, t) {
  const e = [
    c[0] - i[0],
    c[1] - i[1],
    c[2] - i[2]
  ];
  return t[0] * e[0] + t[1] * e[1] + t[2] * e[2] > 0;
}
class Tr {
  static isTransparent(i) {
    return i.transparent && i.opacity < 1;
  }
}
const Pt = class Pt {
  // Copied from three.js source
  // Original source: http://stackoverflow.com/questions/105034/how-to-create-a-guid-uuid-in-javascript/21963136#21963136
  static create() {
    const i = Math.random() * 4294967295 | 0, t = Math.random() * 4294967295 | 0, e = Math.random() * 4294967295 | 0, s = Math.random() * 4294967295 | 0;
    return `${Pt._lut[i & 255] + Pt._lut[i >> 8 & 255] + Pt._lut[i >> 16 & 255] + Pt._lut[i >> 24 & 255]}-${Pt._lut[t & 255]}${Pt._lut[t >> 8 & 255]}-${Pt._lut[t >> 16 & 15 | 64]}${Pt._lut[t >> 24 & 255]}-${Pt._lut[e & 63 | 128]}${Pt._lut[e >> 8 & 255]}-${Pt._lut[e >> 16 & 255]}${Pt._lut[e >> 24 & 255]}${Pt._lut[s & 255]}${Pt._lut[s >> 8 & 255]}${Pt._lut[s >> 16 & 255]}${Pt._lut[s >> 24 & 255]}`.toLowerCase();
  }
  static validate(i) {
    if (!Pt._pattern.test(i))
      throw new Error(
        `${i} is not a valid UUID v4.

- If you're the tool creator, you can take one from https://www.uuidgenerator.net/.

- If you're using a platform tool, verify the uuid isn't misspelled or contact the tool creator.`
      );
  }
};
C(Pt, "_pattern", /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-4[0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/), // prettier-ignore
C(Pt, "_lut", [
  "00",
  "01",
  "02",
  "03",
  "04",
  "05",
  "06",
  "07",
  "08",
  "09",
  "0a",
  "0b",
  "0c",
  "0d",
  "0e",
  "0f",
  "10",
  "11",
  "12",
  "13",
  "14",
  "15",
  "16",
  "17",
  "18",
  "19",
  "1a",
  "1b",
  "1c",
  "1d",
  "1e",
  "1f",
  "20",
  "21",
  "22",
  "23",
  "24",
  "25",
  "26",
  "27",
  "28",
  "29",
  "2a",
  "2b",
  "2c",
  "2d",
  "2e",
  "2f",
  "30",
  "31",
  "32",
  "33",
  "34",
  "35",
  "36",
  "37",
  "38",
  "39",
  "3a",
  "3b",
  "3c",
  "3d",
  "3e",
  "3f",
  "40",
  "41",
  "42",
  "43",
  "44",
  "45",
  "46",
  "47",
  "48",
  "49",
  "4a",
  "4b",
  "4c",
  "4d",
  "4e",
  "4f",
  "50",
  "51",
  "52",
  "53",
  "54",
  "55",
  "56",
  "57",
  "58",
  "59",
  "5a",
  "5b",
  "5c",
  "5d",
  "5e",
  "5f",
  "60",
  "61",
  "62",
  "63",
  "64",
  "65",
  "66",
  "67",
  "68",
  "69",
  "6a",
  "6b",
  "6c",
  "6d",
  "6e",
  "6f",
  "70",
  "71",
  "72",
  "73",
  "74",
  "75",
  "76",
  "77",
  "78",
  "79",
  "7a",
  "7b",
  "7c",
  "7d",
  "7e",
  "7f",
  "80",
  "81",
  "82",
  "83",
  "84",
  "85",
  "86",
  "87",
  "88",
  "89",
  "8a",
  "8b",
  "8c",
  "8d",
  "8e",
  "8f",
  "90",
  "91",
  "92",
  "93",
  "94",
  "95",
  "96",
  "97",
  "98",
  "99",
  "9a",
  "9b",
  "9c",
  "9d",
  "9e",
  "9f",
  "a0",
  "a1",
  "a2",
  "a3",
  "a4",
  "a5",
  "a6",
  "a7",
  "a8",
  "a9",
  "aa",
  "ab",
  "ac",
  "ad",
  "ae",
  "af",
  "b0",
  "b1",
  "b2",
  "b3",
  "b4",
  "b5",
  "b6",
  "b7",
  "b8",
  "b9",
  "ba",
  "bb",
  "bc",
  "bd",
  "be",
  "bf",
  "c0",
  "c1",
  "c2",
  "c3",
  "c4",
  "c5",
  "c6",
  "c7",
  "c8",
  "c9",
  "ca",
  "cb",
  "cc",
  "cd",
  "ce",
  "cf",
  "d0",
  "d1",
  "d2",
  "d3",
  "d4",
  "d5",
  "d6",
  "d7",
  "d8",
  "d9",
  "da",
  "db",
  "dc",
  "dd",
  "de",
  "df",
  "e0",
  "e1",
  "e2",
  "e3",
  "e4",
  "e5",
  "e6",
  "e7",
  "e8",
  "e9",
  "ea",
  "eb",
  "ec",
  "ed",
  "ee",
  "ef",
  "f0",
  "f1",
  "f2",
  "f3",
  "f4",
  "f5",
  "f6",
  "f7",
  "f8",
  "f9",
  "fa",
  "fb",
  "fc",
  "fd",
  "fe",
  "ff"
]);
let oe = Pt;
class Th extends At {
  constructor(t, e) {
    super(t);
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * An event that is triggered when a vertex is found.
     * The event passes a THREE.Vector3 representing the position of the found vertex.
     */
    C(this, "onVertexFound", new j());
    /**
     * An event that is triggered when a vertex is lost.
     * The event passes a THREE.Vector3 representing the position of the lost vertex.
     */
    C(this, "onVertexLost", new j());
    /**
     * An event that is triggered when the picker is enabled or disabled
     */
    C(this, "onEnabled", new j());
    /**
     * A reference to the Components instance associated with this VertexPicker.
     */
    C(this, "components");
    /**
     * A reference to the working plane used for vertex picking.
     * This plane is used to determine which vertices are considered valid for picking.
     * If this value is null, all vertices are considered valid.
     */
    C(this, "workingPlane", null);
    C(this, "_pickedPoint", null);
    C(this, "_config");
    C(this, "_enabled", !1);
    this.components = t, this.config = {
      snapDistance: 0.25,
      showOnlyVertex: !1,
      ...e
    }, this.enabled = !1;
  }
  /**
   * Sets the enabled state of the VertexPicker.
   * When enabled, the VertexPicker will actively search for vertices in the 3D scene.
   * When disabled, the VertexPicker will stop searching for vertices and reset the picked point.
   *
   * @param value - The new enabled state.
   */
  set enabled(t) {
    this._enabled = t, t || (this._pickedPoint = null), this.onEnabled.trigger(t);
  }
  /**
   * Gets the current enabled state of the VertexPicker.
   *
   * @returns The current enabled state.
   */
  get enabled() {
    return this._enabled;
  }
  /**
   * Sets the configuration for the VertexPicker component.
   *
   * @param value - A Partial object containing the configuration properties to update.
   * The properties not provided in the value object will retain their current values.
   *
   * @example
   * ```typescript
   * vertexPicker.config = {
   *   snapDistance: 0.5,
   *   showOnlyVertex: true,
   * };
   * ```
   */
  set config(t) {
    this._config = { ...this._config, ...t };
  }
  /**
   * Gets the current configuration for the VertexPicker component.
   *
   * @returns A copy of the current VertexPickerConfig object.
   *
   * @example
   * ```typescript
   * const currentConfig = vertexPicker.config;
   * console.log(currentConfig.snapDistance); // Output: 0.25
   * ```
   */
  get config() {
    return this._config;
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.onVertexFound.reset(), this.onVertexLost.reset(), this.components = null, this.onDisposed.trigger(), this.onDisposed.reset();
  }
  /**
   * Performs the vertex picking operation based on the current state of the VertexPicker.
   *
   * @param world - The World instance to use for raycasting.
   *
   * @returns The current picked point, or null if no point is picked.
   *
   * @remarks
   * This method checks if the VertexPicker is enabled. If not, it returns the current picked point.
   * If enabled, it performs raycasting to find the closest intersecting object.
   * It then determines the closest vertex or point on the face, based on the configuration settings.
   * If the picked point is on the working plane (if defined), it triggers the `onVertexFound` event and updates the `pickedPoint`.
   * If the picked point is not on the working plane, it resets the `pickedPoint`.
   * If no intersecting object is found, it triggers the `onVertexLost` event and resets the `pickedPoint`.
   */
  get(t) {
    if (!this.enabled)
      return this._pickedPoint;
    const n = this.components.get(ci).get(t).castRay();
    if (!n)
      return this._pickedPoint !== null && (this.onVertexLost.trigger(), this._pickedPoint = null), this._pickedPoint;
    const r = this.getClosestVertex(n);
    return r ? (this.workingPlane ? Math.abs(this.workingPlane.distanceToPoint(r)) < 1e-3 : !0) ? ((this._pickedPoint === null || !this._pickedPoint.equals(r)) && (this._pickedPoint = r.clone(), this.onVertexFound.trigger(this._pickedPoint)), this._pickedPoint) : (this._pickedPoint = null, this._pickedPoint) : (this._pickedPoint !== null && (this.onVertexLost.trigger(), this._pickedPoint = null), this._pickedPoint);
  }
  getClosestVertex(t) {
    let e = new D.Vector3(), s = !1, n = Number.MAX_SAFE_INTEGER;
    const r = this.getVertices(t);
    if (r === null)
      return null;
    for (const o of r) {
      if (!o)
        continue;
      const a = t.point.distanceTo(o);
      a > n || a > this._config.snapDistance || (s = !0, e = o, n = t.point.distanceTo(o));
    }
    return s ? e : this.config.showOnlyVertex ? null : t.point;
  }
  getVertices(t) {
    const e = t.object;
    if (!t.face || !e)
      return null;
    const s = e.geometry, n = new D.Matrix4(), { instanceId: r } = t, o = r !== void 0, a = e instanceof D.InstancedMesh;
    return a && o && e.getMatrixAt(r, n), [
      this.getVertex(t.face.a, s),
      this.getVertex(t.face.b, s),
      this.getVertex(t.face.c, s)
    ].map((l) => (l && (a && o && l.applyMatrix4(n), l.applyMatrix4(e.matrixWorld)), l));
  }
  getVertex(t, e) {
    if (t === void 0)
      return null;
    const s = e.attributes.position;
    return new D.Vector3(
      s.getX(t),
      s.getY(t),
      s.getZ(t)
    );
  }
}
const Fs = class Fs {
  constructor() {
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * The list of components created in this app.
     * The keys are UUIDs and the values are instances of the components.
     */
    C(this, "list", /* @__PURE__ */ new Map());
    /**
     * If disabled, the animation loop will be stopped.
     * Default value is false.
     */
    C(this, "enabled", !1);
    C(this, "_clock");
    /**
     * Event that triggers the Components instance is initialized.
     *
     * @remarks
     * This event is triggered once when the {@link Components.init} method has been called and finish processing.
     * This is useful to set configuration placeholders that need to be executed when the components instance is initialized.
     * For example, enabling and configuring custom effects in a post-production renderer.
     *
     * @example
     * ```typescript
     * const components = new Components();
     * components.onInit.add(() => {
     *   // Enable custom effects in the post-production renderer
     *   // or any other operation dependant on the component initialization
     * });
     * components.init();
     * ```
     */
    C(this, "onInit", new j());
    C(this, "update", () => {
      if (!this.enabled)
        return;
      const i = this._clock.getDelta();
      for (const [t, e] of this.list)
        e.enabled && e.isUpdateable() && e.update(i);
      requestAnimationFrame(this.update);
    });
    this._clock = new D.Clock(), Fs.setupBVH();
  }
  /**
   * Adds a component to the list of components.
   * Throws an error if a component with the same UUID already exists.
   *
   * @param uuid - The unique identifier of the component.
   * @param instance - The instance of the component to be added.
   *
   * @throws Will throw an error if a component with the same UUID already exists.
   *
   * @internal
   */
  add(i, t) {
    if (this.list.has(i))
      throw new Error(
        "You're trying to add a component that already exists in the components instance. Use Components.get() instead."
      );
    oe.validate(i), this.list.set(i, t);
  }
  /**
   * Retrieves a component instance by its constructor function.
   * If the component does not exist in the list, it will be created and added.
   *
   * @template U - The type of the component to retrieve.
   * @param Component - The constructor function of the component to retrieve.
   *
   * @returns The instance of the requested component.
   *
   * @throws Will throw an error if a component with the same UUID already exists.
   *
   * @internal
   */
  get(i) {
    const t = i.uuid;
    if (!this.list.has(t)) {
      const e = new i(this);
      return this.list.has(t) || this.add(t, e), e;
    }
    return this.list.get(t);
  }
  /**
   * Initializes the Components instance.
   * This method starts the animation loop, sets the enabled flag to true,
   * and calls the update method.
   */
  init() {
    this.enabled = !0, this._clock.start(), this.update(), this.onInit.trigger();
  }
  /**
   * Disposes the memory of all the components and tools of this instance of
   * the library. A memory leak will be created if:
   *
   * - An instance of the library ends up out of scope and this function isn't
   * called. This is especially relevant in Single Page Applications (React,
   * Angular, Vue, etc).
   *
   * - Any of the objects of this instance (meshes, geometries,materials, etc) is
   * referenced by a reference type (object or array).
   *
   * You can learn more about how Three.js handles memory leaks
   * [here](https://threejs.org/docs/#manual/en/introduction/How-to-dispose-of-objects).
   *
   */
  dispose() {
    this.enabled = !1;
    for (const [i, t] of this.list)
      t.enabled = !1, t.isDisposeable() && t.dispose();
    this._clock.stop(), this.onDisposed.trigger(), this.onDisposed.reset();
  }
  static setupBVH() {
    D.BufferGeometry.prototype.computeBoundsTree = xa, D.BufferGeometry.prototype.disposeBoundsTree = Ba, D.Mesh.prototype.raycast = Ua;
  }
};
/**
 * The version of the @thatopen/components library.
 */
C(Fs, "release", "2.3.18");
let Cs = Fs;
class Qa extends Vn {
  constructor() {
    super(...arguments);
    /**
     * All the loaded [meshes](https://threejs.org/docs/#api/en/objects/Mesh). These meshes will be taken into account in operations like raycasting.
     */
    C(this, "meshes", /* @__PURE__ */ new Set());
    /** {@link Updateable.onAfterUpdate} */
    C(this, "onAfterUpdate", new j());
    /** {@link Updateable.onBeforeUpdate} */
    C(this, "onBeforeUpdate", new j());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * Indicates whether the world is currently being disposed. This is useful to prevent trying to access world's elements when it's being disposed, which could cause errors when you dispose a world.
     */
    C(this, "isDisposing", !1);
    /**
     * Indicates whether the world is currently enabled.
     * When disabled, the world will not be updated.
     */
    C(this, "enabled", !0);
    /**
     * A unique identifier for the world. Is not meant to be changed at any moment.
     */
    C(this, "uuid", oe.create());
    /**
     * An optional name for the world.
     */
    C(this, "name");
    C(this, "_scene");
    C(this, "_camera");
    C(this, "_renderer", null);
  }
  /**
   * Getter for the scene. If no scene is initialized, it throws an error.
   * @returns The current scene.
   */
  get scene() {
    if (!this._scene)
      throw new Error("No scene initialized!");
    return this._scene;
  }
  /**
   * Setter for the scene. It sets the current scene, adds the world to the scene's worlds set,
   * sets the current world in the scene, and triggers the scene's onWorldChanged event with the added action.
   * @param scene - The new scene to be set.
   */
  set scene(t) {
    this._scene = t, t.worlds.set(this.uuid, this), t.currentWorld = this, t.onWorldChanged.trigger({ world: this, action: "added" });
  }
  /**
   * Getter for the camera. If no camera is initialized, it throws an error.
   * @returns The current camera.
   */
  get camera() {
    if (!this._camera)
      throw new Error("No camera initialized!");
    return this._camera;
  }
  /**
   * Setter for the camera. It sets the current camera, adds the world to the camera's worlds set,
   * sets the current world in the camera, and triggers the camera's onWorldChanged event with the added action.
   * @param camera - The new camera to be set.
   */
  set camera(t) {
    this._camera = t, t.worlds.set(this.uuid, this), t.currentWorld = this, t.onWorldChanged.trigger({ world: this, action: "added" });
  }
  /**
   * Getter for the renderer.
   * @returns The current renderer or null if no renderer is set. Some worlds don't need a renderer to work (when your mail goal is not to display a 3D viewport to the user).
   */
  get renderer() {
    return this._renderer;
  }
  /**
   * Setter for the renderer. It sets the current renderer, adds the world to the renderer's worlds set,
   * sets the current world in the renderer, and triggers the renderer's onWorldChanged event with the added action.
   * If a new renderer is set, it also triggers the onWorldChanged event with the removed action for the old renderer.
   * @param renderer - The new renderer to be set or null to remove the current renderer.
   */
  set renderer(t) {
    this._renderer = t, t && (t.worlds.set(this.uuid, this), t.currentWorld = this, t.onWorldChanged.trigger({ world: this, action: "added" }));
  }
  /** {@link Updateable.update} */
  update(t) {
    this.enabled && (!this._scene || !this._camera || (this.scene.currentWorld = this, this.camera.currentWorld = this, this.renderer && (this.renderer.currentWorld = this), this.onBeforeUpdate.trigger(), this.scene.isUpdateable() && this.scene.update(t), this.camera.isUpdateable() && this.camera.update(t), this.renderer && this.renderer.update(t), this.onAfterUpdate.trigger()));
  }
  /** {@link Disposable.dispose} */
  dispose(t = !0) {
    if (this.enabled = !1, this.isDisposing = !0, this.scene.onWorldChanged.trigger({ world: this, action: "removed" }), this.camera.onWorldChanged.trigger({ world: this, action: "removed" }), this.renderer && this.renderer.onWorldChanged.trigger({ world: this, action: "removed" }), t) {
      const s = this.components.get(De);
      this.scene.dispose(), this.camera.isDisposeable() && this.camera.dispose(), this.renderer && this.renderer.dispose();
      for (const n of this.meshes)
        s.destroy(n);
      this.meshes.clear();
    }
    this._scene = null, this._camera = null, this._renderer = null, this.components.get(Ts).list.delete(this.uuid), this.onDisposed.trigger(), this.onDisposed.reset();
  }
}
class Ge {
  constructor(i, t, e, s) {
    C(this, "_component");
    C(this, "name");
    C(this, "uuid");
    this._component = i, this.name = e, this.uuid = s ?? oe.create(), t.get(Ye).list.set(this.uuid, this);
  }
  get controls() {
    const i = {};
    for (const t in this._config) {
      const e = this._config[t];
      i[t] = this.copyEntry(e);
    }
    return i;
  }
  copyEntry(i) {
    if (i.type === "Boolean") {
      const t = i;
      return {
        type: t.type,
        value: t.value
      };
    }
    if (i.type === "Color") {
      const t = i;
      return {
        type: t.type,
        value: t.value.clone()
      };
    }
    if (i.type === "Text") {
      const t = i;
      return {
        type: t.type,
        value: t.value
      };
    }
    if (i.type === "Number") {
      const t = i;
      return {
        type: t.type,
        value: t.value,
        min: t.min,
        max: t.max,
        interpolable: t.interpolable
      };
    }
    if (i.type === "Select") {
      const t = i;
      return {
        type: t.type,
        value: t.value,
        multiple: t.multiple,
        options: new Set(t.options)
      };
    }
    if (i.type === "Vector3") {
      const t = i;
      return {
        type: t.type,
        value: t.value.clone()
      };
    }
    if (i.type === "TextSet") {
      const t = i;
      return {
        type: t.type,
        value: new Set(t.value)
      };
    }
    if (i.type === "None") {
      const t = i;
      return {
        type: t.type,
        value: t.value
      };
    }
    throw new Error("Invalid entry!");
  }
}
const Ss = class Ss extends At {
  constructor(t) {
    super(t);
    /**
     * The list of all configurations of this app.
     */
    C(this, "list", new re());
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    t.add(Ss.uuid, this);
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(Ss, "uuid", "b8c764e0-6b24-4e77-9a32-35fa728ee5b4");
let Ye = Ss;
class Ka {
  constructor(i, t) {
    C(this, "_list");
    C(this, "_scene");
    this._list = i, this._scene = t;
  }
  get color() {
    return this._list.directionalLight.color.value;
  }
  set color(i) {
    this._list.directionalLight.color.value = i;
    for (const [, t] of this._scene.directionalLights)
      t.color.copy(i);
  }
  get intensity() {
    return this._list.directionalLight.intensity.value;
  }
  set intensity(i) {
    this._list.directionalLight.intensity.value = i;
    for (const [, t] of this._scene.directionalLights)
      t.intensity = i;
  }
  get position() {
    return this._list.directionalLight.position.value.clone();
  }
  set position(i) {
    this._list.directionalLight.position.value = i;
    for (const [, t] of this._scene.directionalLights)
      t.position.copy(i);
  }
}
class Ja {
  constructor(i, t) {
    C(this, "_list");
    C(this, "_scene");
    this._list = i, this._scene = t;
  }
  get color() {
    return this._list.ambientLight.color.value;
  }
  set color(i) {
    this._list.ambientLight.color.value = i;
    for (const [, t] of this._scene.ambientLights)
      t.color.copy(i);
  }
  get intensity() {
    return this._list.ambientLight.intensity.value;
  }
  set intensity(i) {
    this._list.ambientLight.intensity.value = i;
    for (const [, t] of this._scene.ambientLights)
      t.intensity = i;
  }
}
class tc extends Ge {
  constructor() {
    super(...arguments);
    C(this, "_config", {
      backgroundColor: {
        value: new D.Color(),
        type: "Color"
      },
      ambientLight: {
        color: {
          type: "Color",
          value: new D.Color()
        },
        intensity: {
          type: "Number",
          interpolable: !0,
          min: 0,
          max: 10,
          value: 2
        }
      },
      directionalLight: {
        color: {
          type: "Color",
          value: new D.Color()
        },
        intensity: {
          type: "Number",
          interpolable: !0,
          min: 0,
          max: 10,
          value: 2
        },
        position: {
          type: "Vector3",
          value: new D.Vector3()
        }
      }
    });
    C(this, "ambientLight", new Ja(this._config, this._component));
    C(this, "directionalLight", new Ka(this._config, this._component));
  }
  get backgroundColor() {
    return this._config.backgroundColor.value;
  }
  set backgroundColor(t) {
    this._config.backgroundColor.value = t, this._component.three.background = t;
  }
}
class ec extends Ga {
  constructor(t) {
    super(t);
    /** {@link Configurable.onSetup} */
    C(this, "onSetup", new j());
    /** {@link Configurable.isSetup} */
    C(this, "isSetup", !1);
    /**
     * The underlying Three.js scene object.
     * It is used to define the 3D space containing objects, lights, and cameras.
     */
    C(this, "three");
    /** {@link Configurable.config} */
    C(this, "config", new tc(this, this.components, "Scene"));
    C(this, "_defaultConfig", {
      backgroundColor: new D.Color(2107698),
      directionalLight: {
        color: new D.Color("white"),
        intensity: 1.5,
        position: new D.Vector3(5, 10, 3)
      },
      ambientLight: {
        color: new D.Color("white"),
        intensity: 1
      }
    });
    this.three = new D.Scene(), this.three.background = new D.Color(2107698);
  }
  /** {@link Configurable.setup} */
  setup(t) {
    const e = { ...this._defaultConfig, ...t };
    this.config.backgroundColor = e.backgroundColor;
    const s = e.ambientLight;
    this.config.ambientLight.color = s.color, this.config.ambientLight.intensity = s.intensity;
    const n = e.directionalLight;
    this.config.directionalLight.color = n.color, this.config.directionalLight.intensity = n.intensity, this.config.directionalLight.position = n.position, this.deleteAllLights();
    const { color: r, intensity: o } = this.config.directionalLight, a = new D.DirectionalLight(r, o);
    a.position.copy(n.position);
    const { color: l, intensity: h } = this.config.directionalLight, f = new D.AmbientLight(l, h);
    this.three.add(a, f), this.directionalLights.set(a.uuid, a), this.ambientLights.set(f.uuid, f), this.isSetup = !0, this.onSetup.trigger();
  }
  dispose() {
    super.dispose(), this.components.get(Ye).list.delete(this.config.uuid);
  }
}
class mh extends Ya {
  /**
   * Constructor for the SimpleRenderer class.
   *
   * @param components - The components instance.
   * @param container - The HTML container where the THREE.js canvas will be rendered.
   * @param parameters - Optional parameters for the THREE.js WebGLRenderer.
   */
  constructor(t, e, s) {
    super(t);
    /**
     * Indicates whether the renderer is enabled. If it's not, it won't be updated.
     * Default is `true`.
     */
    C(this, "enabled", !0);
    /**
     * The HTML container of the THREE.js canvas where the scene is rendered.
     */
    C(this, "container");
    /**
     * The THREE.js WebGLRenderer instance.
     */
    C(this, "three");
    C(this, "_canvas");
    C(this, "_parameters");
    C(this, "_resizeObserver", null);
    C(this, "onContainerUpdated", new j());
    C(this, "_resizing", !1);
    /** {@link Resizeable.resize} */
    C(this, "resize", (t) => {
      if (this._resizing)
        return;
      this._resizing = !0, this.onContainerUpdated.trigger();
      const e = t ? t.x : this.container.clientWidth, s = t ? t.y : this.container.clientHeight;
      this.three.setSize(e, s), this.onResize.trigger(new D.Vector2(e, s)), this._resizing = !1;
    });
    C(this, "resizeEvent", () => {
      this.resize();
    });
    C(this, "onContextLost", (t) => {
      t.preventDefault(), this.enabled = !1;
    });
    C(this, "onContextBack", () => {
      this.three.setRenderTarget(null), this.three.dispose(), this.three = new D.WebGLRenderer({
        canvas: this._canvas,
        antialias: !0,
        alpha: !0,
        ...this._parameters
      }), this.enabled = !0;
    });
    this.container = e, this._parameters = s, this.three = new D.WebGLRenderer({
      antialias: !0,
      alpha: !0,
      ...s
    }), this.three.setPixelRatio(Math.min(window.devicePixelRatio, 2)), this.setupRenderer(), this.setupEvents(!0), this.resize(), this._canvas = this.three.domElement;
    const n = this.three.getContext(), { canvas: r } = n;
    r.addEventListener("webglcontextlost", this.onContextLost, !1), r.addEventListener("webglcontextrestored", this.onContextBack, !1);
  }
  /** {@link Updateable.update} */
  update() {
    if (!this.enabled || !this.currentWorld)
      return;
    this.onBeforeUpdate.trigger(this);
    const t = this.currentWorld.scene.three, e = this.currentWorld.camera.three;
    this.three.render(t, e), this.onAfterUpdate.trigger(this);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.enabled = !1, this.setupEvents(!1), this.three.domElement.remove(), this.three.forceContextLoss(), this.three.dispose(), this.onResize.reset(), this.onAfterUpdate.reset(), this.onBeforeUpdate.reset(), this.onDisposed.trigger(), this.onDisposed.reset();
  }
  /** {@link Resizeable.getSize}. */
  getSize() {
    return new D.Vector2(
      this.three.domElement.clientWidth,
      this.three.domElement.clientHeight
    );
  }
  /**
   * Sets up and manages the event listeners for the renderer.
   *
   * @param active - A boolean indicating whether to activate or deactivate the event listeners.
   *
   * @throws Will throw an error if the renderer does not have an HTML container.
   */
  setupEvents(t) {
    const e = this.three.domElement.parentElement;
    if (!e)
      throw new Error("This renderer needs to have an HTML container!");
    this._resizeObserver && (this._resizeObserver.disconnect(), this._resizeObserver = null), window.removeEventListener("resize", this.resizeEvent), t && (this._resizeObserver = new ResizeObserver(this.resizeEvent), this._resizeObserver.observe(e), window.addEventListener("resize", this.resizeEvent));
  }
  setupRenderer() {
    this.three.localClippingEnabled = !0, this.container && this.container.appendChild(this.three.domElement), this.onContainerUpdated.trigger();
  }
}
/*!
 * camera-controls
 * https://github.com/yomotsu/camera-controls
 * (c) 2017 @yomotsu
 * Released under the MIT License.
 */
const ht = {
  LEFT: 1,
  RIGHT: 2,
  MIDDLE: 4
}, H = Object.freeze({
  NONE: 0,
  ROTATE: 1,
  TRUCK: 2,
  OFFSET: 4,
  DOLLY: 8,
  ZOOM: 16,
  TOUCH_ROTATE: 32,
  TOUCH_TRUCK: 64,
  TOUCH_OFFSET: 128,
  TOUCH_DOLLY: 256,
  TOUCH_ZOOM: 512,
  TOUCH_DOLLY_TRUCK: 1024,
  TOUCH_DOLLY_OFFSET: 2048,
  TOUCH_DOLLY_ROTATE: 4096,
  TOUCH_ZOOM_TRUCK: 8192,
  TOUCH_ZOOM_OFFSET: 16384,
  TOUCH_ZOOM_ROTATE: 32768
}), Ke = {
  NONE: 0,
  IN: 1,
  OUT: -1
};
function ve(c) {
  return c.isPerspectiveCamera;
}
function Ne(c) {
  return c.isOrthographicCamera;
}
const Je = Math.PI * 2, mr = Math.PI / 2, io = 1e-5, Ri = Math.PI / 180;
function Jt(c, i, t) {
  return Math.max(i, Math.min(t, c));
}
function gt(c, i = io) {
  return Math.abs(c) < i;
}
function pt(c, i, t = io) {
  return gt(c - i, t);
}
function Rr(c, i) {
  return Math.round(c / i) * i;
}
function gi(c) {
  return isFinite(c) ? c : c < 0 ? -Number.MAX_VALUE : Number.MAX_VALUE;
}
function Ai(c) {
  return Math.abs(c) < Number.MAX_VALUE ? c : c * (1 / 0);
}
function ns(c, i, t, e, s = 1 / 0, n) {
  e = Math.max(1e-4, e);
  const r = 2 / e, o = r * n, a = 1 / (1 + o + 0.48 * o * o + 0.235 * o * o * o);
  let l = c - i;
  const h = i, f = s * e;
  l = Jt(l, -f, f), i = c - l;
  const I = (t.value + r * l) * n;
  t.value = (t.value - r * I) * a;
  let u = i + (l + I) * a;
  return h - c > 0 == u > h && (u = h, t.value = (u - h) / n), u;
}
function gr(c, i, t, e, s = 1 / 0, n, r) {
  e = Math.max(1e-4, e);
  const o = 2 / e, a = o * n, l = 1 / (1 + a + 0.48 * a * a + 0.235 * a * a * a);
  let h = i.x, f = i.y, I = i.z, u = c.x - h, d = c.y - f, E = c.z - I;
  const T = h, p = f, R = I, S = s * e, m = S * S, F = u * u + d * d + E * E;
  if (F > m) {
    const v = Math.sqrt(F);
    u = u / v * S, d = d / v * S, E = E / v * S;
  }
  h = c.x - u, f = c.y - d, I = c.z - E;
  const O = (t.x + o * u) * n, y = (t.y + o * d) * n, w = (t.z + o * E) * n;
  t.x = (t.x - o * O) * l, t.y = (t.y - o * y) * l, t.z = (t.z - o * w) * l, r.x = h + (u + O) * l, r.y = f + (d + y) * l, r.z = I + (E + w) * l;
  const L = T - c.x, b = p - c.y, Y = R - c.z, N = r.x - T, M = r.y - p, g = r.z - R;
  return L * N + b * M + Y * g > 0 && (r.x = T, r.y = p, r.z = R, t.x = (r.x - T) / n, t.y = (r.y - p) / n, t.z = (r.z - R) / n), r;
}
function on(c, i) {
  i.set(0, 0), c.forEach((t) => {
    i.x += t.clientX, i.y += t.clientY;
  }), i.x /= c.length, i.y /= c.length;
}
function an(c, i) {
  return Ne(c) ? (console.warn(`${i} is not supported in OrthographicCamera`), !0) : !1;
}
class ic {
  constructor() {
    this._listeners = {};
  }
  /**
   * Adds the specified event listener.
   * @param type event name
   * @param listener handler function
   * @category Methods
   */
  addEventListener(i, t) {
    const e = this._listeners;
    e[i] === void 0 && (e[i] = []), e[i].indexOf(t) === -1 && e[i].push(t);
  }
  /**
   * Presence of the specified event listener.
   * @param type event name
   * @param listener handler function
   * @category Methods
   */
  hasEventListener(i, t) {
    const e = this._listeners;
    return e[i] !== void 0 && e[i].indexOf(t) !== -1;
  }
  /**
   * Removes the specified event listener
   * @param type event name
   * @param listener handler function
   * @category Methods
   */
  removeEventListener(i, t) {
    const s = this._listeners[i];
    if (s !== void 0) {
      const n = s.indexOf(t);
      n !== -1 && s.splice(n, 1);
    }
  }
  /**
   * Removes all event listeners
   * @param type event name
   * @category Methods
   */
  removeAllEventListeners(i) {
    if (!i) {
      this._listeners = {};
      return;
    }
    Array.isArray(this._listeners[i]) && (this._listeners[i].length = 0);
  }
  /**
   * Fire an event type.
   * @param event DispatcherEvent
   * @category Methods
   */
  dispatchEvent(i) {
    const e = this._listeners[i.type];
    if (e !== void 0) {
      i.target = this;
      const s = e.slice(0);
      for (let n = 0, r = s.length; n < r; n++)
        s[n].call(this, i);
    }
  }
}
const sc = "2.7.3", rs = 1 / 8, so = typeof window < "u", nc = so && /Mac/.test(navigator.platform), rc = !(so && "PointerEvent" in window);
let lt, Ar, os, cn, Yt, ut, Et, ti, Fi, le, he, Ue, Fr, Sr, Xt, Si, ei, Or, ln, Nr, hn, un, as;
class vt extends ic {
  /**
       * Injects THREE as the dependency. You can then proceed to use CameraControls.
       *
       * e.g
       * ```javascript
       * CameraControls.install( { THREE: THREE } );
       * ```
       *
       * Note: If you do not wish to use enter three.js to reduce file size(tree-shaking for example), make a subset to install.
       *
       * ```js
       * import {
       * 	Vector2,
       * 	Vector3,
       * 	Vector4,
       * 	Quaternion,
       * 	Matrix4,
       * 	Spherical,
       * 	Box3,
       * 	Sphere,
       * 	Raycaster,
       * 	MathUtils,
       * } from 'three';
       *
       * const subsetOfTHREE = {
       * 	Vector2   : Vector2,
       * 	Vector3   : Vector3,
       * 	Vector4   : Vector4,
       * 	Quaternion: Quaternion,
       * 	Matrix4   : Matrix4,
       * 	Spherical : Spherical,
       * 	Box3      : Box3,
       * 	Sphere    : Sphere,
       * 	Raycaster : Raycaster,
       * };
  
       * CameraControls.install( { THREE: subsetOfTHREE } );
       * ```
       * @category Statics
       */
  static install(i) {
    lt = i.THREE, Ar = Object.freeze(new lt.Vector3(0, 0, 0)), os = Object.freeze(new lt.Vector3(0, 1, 0)), cn = Object.freeze(new lt.Vector3(0, 0, 1)), Yt = new lt.Vector2(), ut = new lt.Vector3(), Et = new lt.Vector3(), ti = new lt.Vector3(), Fi = new lt.Vector3(), le = new lt.Vector3(), he = new lt.Vector3(), Ue = new lt.Vector3(), Fr = new lt.Vector3(), Sr = new lt.Vector3(), Xt = new lt.Spherical(), Si = new lt.Spherical(), ei = new lt.Box3(), Or = new lt.Box3(), ln = new lt.Sphere(), Nr = new lt.Quaternion(), hn = new lt.Quaternion(), un = new lt.Matrix4(), as = new lt.Raycaster();
  }
  /**
   * list all ACTIONs
   * @category Statics
   */
  static get ACTION() {
    return H;
  }
  /**
   * Creates a `CameraControls` instance.
   *
   * Note:
   * You **must install** three.js before using camera-controls. see [#install](#install)
   * Not doing so will lead to runtime errors (`undefined` references to THREE).
   *
   * e.g.
   * ```
   * CameraControls.install( { THREE } );
   * const cameraControls = new CameraControls( camera, domElement );
   * ```
   *
   * @param camera A `THREE.PerspectiveCamera` or `THREE.OrthographicCamera` to be controlled.
   * @param domElement A `HTMLElement` for the draggable area, usually `renderer.domElement`.
   * @category Constructor
   */
  constructor(i, t) {
    super(), this.minPolarAngle = 0, this.maxPolarAngle = Math.PI, this.minAzimuthAngle = -1 / 0, this.maxAzimuthAngle = 1 / 0, this.minDistance = Number.EPSILON, this.maxDistance = 1 / 0, this.infinityDolly = !1, this.minZoom = 0.01, this.maxZoom = 1 / 0, this.smoothTime = 0.25, this.draggingSmoothTime = 0.125, this.maxSpeed = 1 / 0, this.azimuthRotateSpeed = 1, this.polarRotateSpeed = 1, this.dollySpeed = 1, this.dollyDragInverted = !1, this.truckSpeed = 2, this.dollyToCursor = !1, this.dragToOffset = !1, this.verticalDragToForward = !1, this.boundaryFriction = 0, this.restThreshold = 0.01, this.colliderMeshes = [], this.cancel = () => {
    }, this._enabled = !0, this._state = H.NONE, this._viewport = null, this._changedDolly = 0, this._changedZoom = 0, this._hasRested = !0, this._boundaryEnclosesCamera = !1, this._needsUpdate = !0, this._updatedLastTime = !1, this._elementRect = new DOMRect(), this._isDragging = !1, this._dragNeedsUpdate = !0, this._activePointers = [], this._lockedPointer = null, this._interactiveArea = new DOMRect(0, 0, 1, 1), this._isUserControllingRotate = !1, this._isUserControllingDolly = !1, this._isUserControllingTruck = !1, this._isUserControllingOffset = !1, this._isUserControllingZoom = !1, this._lastDollyDirection = Ke.NONE, this._thetaVelocity = { value: 0 }, this._phiVelocity = { value: 0 }, this._radiusVelocity = { value: 0 }, this._targetVelocity = new lt.Vector3(), this._focalOffsetVelocity = new lt.Vector3(), this._zoomVelocity = { value: 0 }, this._truckInternal = (m, F, O) => {
      let y, w;
      if (ve(this._camera)) {
        const L = ut.copy(this._camera.position).sub(this._target), b = this._camera.getEffectiveFOV() * Ri, Y = L.length() * Math.tan(b * 0.5);
        y = this.truckSpeed * m * Y / this._elementRect.height, w = this.truckSpeed * F * Y / this._elementRect.height;
      } else if (Ne(this._camera)) {
        const L = this._camera;
        y = m * (L.right - L.left) / L.zoom / this._elementRect.width, w = F * (L.top - L.bottom) / L.zoom / this._elementRect.height;
      } else
        return;
      this.verticalDragToForward ? (O ? this.setFocalOffset(this._focalOffsetEnd.x + y, this._focalOffsetEnd.y, this._focalOffsetEnd.z, !0) : this.truck(y, 0, !0), this.forward(-w, !0)) : O ? this.setFocalOffset(this._focalOffsetEnd.x + y, this._focalOffsetEnd.y + w, this._focalOffsetEnd.z, !0) : this.truck(y, w, !0);
    }, this._rotateInternal = (m, F) => {
      const O = Je * this.azimuthRotateSpeed * m / this._elementRect.height, y = Je * this.polarRotateSpeed * F / this._elementRect.height;
      this.rotate(O, y, !0);
    }, this._dollyInternal = (m, F, O) => {
      const y = Math.pow(0.95, -m * this.dollySpeed), w = this._sphericalEnd.radius, L = this._sphericalEnd.radius * y, b = Jt(L, this.minDistance, this.maxDistance), Y = b - L;
      this.infinityDolly && this.dollyToCursor ? this._dollyToNoClamp(L, !0) : this.infinityDolly && !this.dollyToCursor ? (this.dollyInFixed(Y, !0), this._dollyToNoClamp(b, !0)) : this._dollyToNoClamp(b, !0), this.dollyToCursor && (this._changedDolly += (this.infinityDolly ? L : b) - w, this._dollyControlCoord.set(F, O)), this._lastDollyDirection = Math.sign(-m);
    }, this._zoomInternal = (m, F, O) => {
      const y = Math.pow(0.95, m * this.dollySpeed), w = this._zoom, L = this._zoom * y;
      this.zoomTo(L, !0), this.dollyToCursor && (this._changedZoom += L - w, this._dollyControlCoord.set(F, O));
    }, typeof lt > "u" && console.error("camera-controls: `THREE` is undefined. You must first run `CameraControls.install( { THREE: THREE } )`. Check the docs for further information."), this._camera = i, this._yAxisUpSpace = new lt.Quaternion().setFromUnitVectors(this._camera.up, os), this._yAxisUpSpaceInverse = this._yAxisUpSpace.clone().invert(), this._state = H.NONE, this._target = new lt.Vector3(), this._targetEnd = this._target.clone(), this._focalOffset = new lt.Vector3(), this._focalOffsetEnd = this._focalOffset.clone(), this._spherical = new lt.Spherical().setFromVector3(ut.copy(this._camera.position).applyQuaternion(this._yAxisUpSpace)), this._sphericalEnd = this._spherical.clone(), this._lastDistance = this._spherical.radius, this._zoom = this._camera.zoom, this._zoomEnd = this._zoom, this._lastZoom = this._zoom, this._nearPlaneCorners = [
      new lt.Vector3(),
      new lt.Vector3(),
      new lt.Vector3(),
      new lt.Vector3()
    ], this._updateNearPlaneCorners(), this._boundary = new lt.Box3(new lt.Vector3(-1 / 0, -1 / 0, -1 / 0), new lt.Vector3(1 / 0, 1 / 0, 1 / 0)), this._cameraUp0 = this._camera.up.clone(), this._target0 = this._target.clone(), this._position0 = this._camera.position.clone(), this._zoom0 = this._zoom, this._focalOffset0 = this._focalOffset.clone(), this._dollyControlCoord = new lt.Vector2(), this.mouseButtons = {
      left: H.ROTATE,
      middle: H.DOLLY,
      right: H.TRUCK,
      wheel: ve(this._camera) ? H.DOLLY : Ne(this._camera) ? H.ZOOM : H.NONE
    }, this.touches = {
      one: H.TOUCH_ROTATE,
      two: ve(this._camera) ? H.TOUCH_DOLLY_TRUCK : Ne(this._camera) ? H.TOUCH_ZOOM_TRUCK : H.NONE,
      three: H.TOUCH_TRUCK
    };
    const e = new lt.Vector2(), s = new lt.Vector2(), n = new lt.Vector2(), r = (m) => {
      if (!this._enabled || !this._domElement)
        return;
      if (this._interactiveArea.left !== 0 || this._interactiveArea.top !== 0 || this._interactiveArea.width !== 1 || this._interactiveArea.height !== 1) {
        const y = this._domElement.getBoundingClientRect(), w = m.clientX / y.width, L = m.clientY / y.height;
        if (w < this._interactiveArea.left || w > this._interactiveArea.right || L < this._interactiveArea.top || L > this._interactiveArea.bottom)
          return;
      }
      const F = m.pointerType !== "mouse" ? null : (m.buttons & ht.LEFT) === ht.LEFT ? ht.LEFT : (m.buttons & ht.MIDDLE) === ht.MIDDLE ? ht.MIDDLE : (m.buttons & ht.RIGHT) === ht.RIGHT ? ht.RIGHT : null;
      if (F !== null) {
        const y = this._findPointerByMouseButton(F);
        y && this._disposePointer(y);
      }
      if ((m.buttons & ht.LEFT) === ht.LEFT && this._lockedPointer)
        return;
      const O = {
        pointerId: m.pointerId,
        clientX: m.clientX,
        clientY: m.clientY,
        deltaX: 0,
        deltaY: 0,
        mouseButton: F
      };
      this._activePointers.push(O), this._domElement.ownerDocument.removeEventListener("pointermove", a, { passive: !1 }), this._domElement.ownerDocument.removeEventListener("pointerup", h), this._domElement.ownerDocument.addEventListener("pointermove", a, { passive: !1 }), this._domElement.ownerDocument.addEventListener("pointerup", h), this._isDragging = !0, E(m);
    }, o = (m) => {
      if (!this._enabled || !this._domElement || this._lockedPointer)
        return;
      if (this._interactiveArea.left !== 0 || this._interactiveArea.top !== 0 || this._interactiveArea.width !== 1 || this._interactiveArea.height !== 1) {
        const y = this._domElement.getBoundingClientRect(), w = m.clientX / y.width, L = m.clientY / y.height;
        if (w < this._interactiveArea.left || w > this._interactiveArea.right || L < this._interactiveArea.top || L > this._interactiveArea.bottom)
          return;
      }
      const F = (m.buttons & ht.LEFT) === ht.LEFT ? ht.LEFT : (m.buttons & ht.MIDDLE) === ht.MIDDLE ? ht.MIDDLE : (m.buttons & ht.RIGHT) === ht.RIGHT ? ht.RIGHT : null;
      if (F !== null) {
        const y = this._findPointerByMouseButton(F);
        y && this._disposePointer(y);
      }
      const O = {
        pointerId: 1,
        clientX: m.clientX,
        clientY: m.clientY,
        deltaX: 0,
        deltaY: 0,
        mouseButton: (m.buttons & ht.LEFT) === ht.LEFT ? ht.LEFT : (m.buttons & ht.MIDDLE) === ht.LEFT ? ht.MIDDLE : (m.buttons & ht.RIGHT) === ht.LEFT ? ht.RIGHT : null
      };
      this._activePointers.push(O), this._domElement.ownerDocument.removeEventListener("mousemove", l), this._domElement.ownerDocument.removeEventListener("mouseup", f), this._domElement.ownerDocument.addEventListener("mousemove", l), this._domElement.ownerDocument.addEventListener("mouseup", f), this._isDragging = !0, E(m);
    }, a = (m) => {
      m.cancelable && m.preventDefault();
      const F = m.pointerId, O = this._lockedPointer || this._findPointerById(F);
      if (O) {
        if (O.clientX = m.clientX, O.clientY = m.clientY, O.deltaX = m.movementX, O.deltaY = m.movementY, this._state = 0, m.pointerType === "touch")
          switch (this._activePointers.length) {
            case 1:
              this._state = this.touches.one;
              break;
            case 2:
              this._state = this.touches.two;
              break;
            case 3:
              this._state = this.touches.three;
              break;
          }
        else
          (!this._isDragging && this._lockedPointer || this._isDragging && (m.buttons & ht.LEFT) === ht.LEFT) && (this._state = this._state | this.mouseButtons.left), this._isDragging && (m.buttons & ht.MIDDLE) === ht.MIDDLE && (this._state = this._state | this.mouseButtons.middle), this._isDragging && (m.buttons & ht.RIGHT) === ht.RIGHT && (this._state = this._state | this.mouseButtons.right);
        T();
      }
    }, l = (m) => {
      const F = this._lockedPointer || this._findPointerById(1);
      F && (F.clientX = m.clientX, F.clientY = m.clientY, F.deltaX = m.movementX, F.deltaY = m.movementY, this._state = 0, (this._lockedPointer || (m.buttons & ht.LEFT) === ht.LEFT) && (this._state = this._state | this.mouseButtons.left), (m.buttons & ht.MIDDLE) === ht.MIDDLE && (this._state = this._state | this.mouseButtons.middle), (m.buttons & ht.RIGHT) === ht.RIGHT && (this._state = this._state | this.mouseButtons.right), T());
    }, h = (m) => {
      const F = this._findPointerById(m.pointerId);
      if (!(F && F === this._lockedPointer)) {
        if (F && this._disposePointer(F), m.pointerType === "touch")
          switch (this._activePointers.length) {
            case 0:
              this._state = H.NONE;
              break;
            case 1:
              this._state = this.touches.one;
              break;
            case 2:
              this._state = this.touches.two;
              break;
            case 3:
              this._state = this.touches.three;
              break;
          }
        else
          this._state = H.NONE;
        p();
      }
    }, f = () => {
      const m = this._findPointerById(1);
      m && m === this._lockedPointer || (m && this._disposePointer(m), this._state = H.NONE, p());
    };
    let I = -1;
    const u = (m) => {
      if (!this._domElement || !this._enabled || this.mouseButtons.wheel === H.NONE)
        return;
      if (this._interactiveArea.left !== 0 || this._interactiveArea.top !== 0 || this._interactiveArea.width !== 1 || this._interactiveArea.height !== 1) {
        const L = this._domElement.getBoundingClientRect(), b = m.clientX / L.width, Y = m.clientY / L.height;
        if (b < this._interactiveArea.left || b > this._interactiveArea.right || Y < this._interactiveArea.top || Y > this._interactiveArea.bottom)
          return;
      }
      if (m.preventDefault(), this.dollyToCursor || this.mouseButtons.wheel === H.ROTATE || this.mouseButtons.wheel === H.TRUCK) {
        const L = performance.now();
        I - L < 1e3 && this._getClientRect(this._elementRect), I = L;
      }
      const F = nc ? -1 : -3, O = m.deltaMode === 1 ? m.deltaY / F : m.deltaY / (F * 10), y = this.dollyToCursor ? (m.clientX - this._elementRect.x) / this._elementRect.width * 2 - 1 : 0, w = this.dollyToCursor ? (m.clientY - this._elementRect.y) / this._elementRect.height * -2 + 1 : 0;
      switch (this.mouseButtons.wheel) {
        case H.ROTATE: {
          this._rotateInternal(m.deltaX, m.deltaY), this._isUserControllingRotate = !0;
          break;
        }
        case H.TRUCK: {
          this._truckInternal(m.deltaX, m.deltaY, !1), this._isUserControllingTruck = !0;
          break;
        }
        case H.OFFSET: {
          this._truckInternal(m.deltaX, m.deltaY, !0), this._isUserControllingOffset = !0;
          break;
        }
        case H.DOLLY: {
          this._dollyInternal(-O, y, w), this._isUserControllingDolly = !0;
          break;
        }
        case H.ZOOM: {
          this._zoomInternal(-O, y, w), this._isUserControllingZoom = !0;
          break;
        }
      }
      this.dispatchEvent({ type: "control" });
    }, d = (m) => {
      if (!(!this._domElement || !this._enabled)) {
        if (this.mouseButtons.right === vt.ACTION.NONE) {
          const F = m instanceof PointerEvent ? m.pointerId : (m instanceof MouseEvent, 0), O = this._findPointerById(F);
          O && this._disposePointer(O), this._domElement.ownerDocument.removeEventListener("pointermove", a, { passive: !1 }), this._domElement.ownerDocument.removeEventListener("pointerup", h), this._domElement.ownerDocument.removeEventListener("mousemove", l), this._domElement.ownerDocument.removeEventListener("mouseup", f);
          return;
        }
        m.preventDefault();
      }
    }, E = (m) => {
      if (!this._enabled)
        return;
      if (on(this._activePointers, Yt), this._getClientRect(this._elementRect), e.copy(Yt), s.copy(Yt), this._activePointers.length >= 2) {
        const O = Yt.x - this._activePointers[1].clientX, y = Yt.y - this._activePointers[1].clientY, w = Math.sqrt(O * O + y * y);
        n.set(0, w);
        const L = (this._activePointers[0].clientX + this._activePointers[1].clientX) * 0.5, b = (this._activePointers[0].clientY + this._activePointers[1].clientY) * 0.5;
        s.set(L, b);
      }
      if (this._state = 0, !m)
        this._lockedPointer && (this._state = this._state | this.mouseButtons.left);
      else if ("pointerType" in m && m.pointerType === "touch")
        switch (this._activePointers.length) {
          case 1:
            this._state = this.touches.one;
            break;
          case 2:
            this._state = this.touches.two;
            break;
          case 3:
            this._state = this.touches.three;
            break;
        }
      else
        !this._lockedPointer && (m.buttons & ht.LEFT) === ht.LEFT && (this._state = this._state | this.mouseButtons.left), (m.buttons & ht.MIDDLE) === ht.MIDDLE && (this._state = this._state | this.mouseButtons.middle), (m.buttons & ht.RIGHT) === ht.RIGHT && (this._state = this._state | this.mouseButtons.right);
      ((this._state & H.ROTATE) === H.ROTATE || (this._state & H.TOUCH_ROTATE) === H.TOUCH_ROTATE || (this._state & H.TOUCH_DOLLY_ROTATE) === H.TOUCH_DOLLY_ROTATE || (this._state & H.TOUCH_ZOOM_ROTATE) === H.TOUCH_ZOOM_ROTATE) && (this._sphericalEnd.theta = this._spherical.theta, this._sphericalEnd.phi = this._spherical.phi, this._thetaVelocity.value = 0, this._phiVelocity.value = 0), ((this._state & H.TRUCK) === H.TRUCK || (this._state & H.TOUCH_TRUCK) === H.TOUCH_TRUCK || (this._state & H.TOUCH_DOLLY_TRUCK) === H.TOUCH_DOLLY_TRUCK || (this._state & H.TOUCH_ZOOM_TRUCK) === H.TOUCH_ZOOM_TRUCK) && (this._targetEnd.copy(this._target), this._targetVelocity.set(0, 0, 0)), ((this._state & H.DOLLY) === H.DOLLY || (this._state & H.TOUCH_DOLLY) === H.TOUCH_DOLLY || (this._state & H.TOUCH_DOLLY_TRUCK) === H.TOUCH_DOLLY_TRUCK || (this._state & H.TOUCH_DOLLY_OFFSET) === H.TOUCH_DOLLY_OFFSET || (this._state & H.TOUCH_DOLLY_ROTATE) === H.TOUCH_DOLLY_ROTATE) && (this._sphericalEnd.radius = this._spherical.radius, this._radiusVelocity.value = 0), ((this._state & H.ZOOM) === H.ZOOM || (this._state & H.TOUCH_ZOOM) === H.TOUCH_ZOOM || (this._state & H.TOUCH_ZOOM_TRUCK) === H.TOUCH_ZOOM_TRUCK || (this._state & H.TOUCH_ZOOM_OFFSET) === H.TOUCH_ZOOM_OFFSET || (this._state & H.TOUCH_ZOOM_ROTATE) === H.TOUCH_ZOOM_ROTATE) && (this._zoomEnd = this._zoom, this._zoomVelocity.value = 0), ((this._state & H.OFFSET) === H.OFFSET || (this._state & H.TOUCH_OFFSET) === H.TOUCH_OFFSET || (this._state & H.TOUCH_DOLLY_OFFSET) === H.TOUCH_DOLLY_OFFSET || (this._state & H.TOUCH_ZOOM_OFFSET) === H.TOUCH_ZOOM_OFFSET) && (this._focalOffsetEnd.copy(this._focalOffset), this._focalOffsetVelocity.set(0, 0, 0)), this.dispatchEvent({ type: "controlstart" });
    }, T = () => {
      if (!this._enabled || !this._dragNeedsUpdate)
        return;
      this._dragNeedsUpdate = !1, on(this._activePointers, Yt);
      const F = this._domElement && document.pointerLockElement === this._domElement ? this._lockedPointer || this._activePointers[0] : null, O = F ? -F.deltaX : s.x - Yt.x, y = F ? -F.deltaY : s.y - Yt.y;
      if (s.copy(Yt), ((this._state & H.ROTATE) === H.ROTATE || (this._state & H.TOUCH_ROTATE) === H.TOUCH_ROTATE || (this._state & H.TOUCH_DOLLY_ROTATE) === H.TOUCH_DOLLY_ROTATE || (this._state & H.TOUCH_ZOOM_ROTATE) === H.TOUCH_ZOOM_ROTATE) && (this._rotateInternal(O, y), this._isUserControllingRotate = !0), (this._state & H.DOLLY) === H.DOLLY || (this._state & H.ZOOM) === H.ZOOM) {
        const w = this.dollyToCursor ? (e.x - this._elementRect.x) / this._elementRect.width * 2 - 1 : 0, L = this.dollyToCursor ? (e.y - this._elementRect.y) / this._elementRect.height * -2 + 1 : 0, b = this.dollyDragInverted ? -1 : 1;
        (this._state & H.DOLLY) === H.DOLLY ? (this._dollyInternal(b * y * rs, w, L), this._isUserControllingDolly = !0) : (this._zoomInternal(b * y * rs, w, L), this._isUserControllingZoom = !0);
      }
      if ((this._state & H.TOUCH_DOLLY) === H.TOUCH_DOLLY || (this._state & H.TOUCH_ZOOM) === H.TOUCH_ZOOM || (this._state & H.TOUCH_DOLLY_TRUCK) === H.TOUCH_DOLLY_TRUCK || (this._state & H.TOUCH_ZOOM_TRUCK) === H.TOUCH_ZOOM_TRUCK || (this._state & H.TOUCH_DOLLY_OFFSET) === H.TOUCH_DOLLY_OFFSET || (this._state & H.TOUCH_ZOOM_OFFSET) === H.TOUCH_ZOOM_OFFSET || (this._state & H.TOUCH_DOLLY_ROTATE) === H.TOUCH_DOLLY_ROTATE || (this._state & H.TOUCH_ZOOM_ROTATE) === H.TOUCH_ZOOM_ROTATE) {
        const w = Yt.x - this._activePointers[1].clientX, L = Yt.y - this._activePointers[1].clientY, b = Math.sqrt(w * w + L * L), Y = n.y - b;
        n.set(0, b);
        const N = this.dollyToCursor ? (s.x - this._elementRect.x) / this._elementRect.width * 2 - 1 : 0, M = this.dollyToCursor ? (s.y - this._elementRect.y) / this._elementRect.height * -2 + 1 : 0;
        (this._state & H.TOUCH_DOLLY) === H.TOUCH_DOLLY || (this._state & H.TOUCH_DOLLY_ROTATE) === H.TOUCH_DOLLY_ROTATE || (this._state & H.TOUCH_DOLLY_TRUCK) === H.TOUCH_DOLLY_TRUCK || (this._state & H.TOUCH_DOLLY_OFFSET) === H.TOUCH_DOLLY_OFFSET ? (this._dollyInternal(Y * rs, N, M), this._isUserControllingDolly = !0) : (this._zoomInternal(Y * rs, N, M), this._isUserControllingZoom = !0);
      }
      ((this._state & H.TRUCK) === H.TRUCK || (this._state & H.TOUCH_TRUCK) === H.TOUCH_TRUCK || (this._state & H.TOUCH_DOLLY_TRUCK) === H.TOUCH_DOLLY_TRUCK || (this._state & H.TOUCH_ZOOM_TRUCK) === H.TOUCH_ZOOM_TRUCK) && (this._truckInternal(O, y, !1), this._isUserControllingTruck = !0), ((this._state & H.OFFSET) === H.OFFSET || (this._state & H.TOUCH_OFFSET) === H.TOUCH_OFFSET || (this._state & H.TOUCH_DOLLY_OFFSET) === H.TOUCH_DOLLY_OFFSET || (this._state & H.TOUCH_ZOOM_OFFSET) === H.TOUCH_ZOOM_OFFSET) && (this._truckInternal(O, y, !0), this._isUserControllingOffset = !0), this.dispatchEvent({ type: "control" });
    }, p = () => {
      on(this._activePointers, Yt), s.copy(Yt), this._dragNeedsUpdate = !1, (this._activePointers.length === 0 || this._activePointers.length === 1 && this._activePointers[0] === this._lockedPointer) && (this._isDragging = !1), this._activePointers.length === 0 && this._domElement && (this._domElement.ownerDocument.removeEventListener("pointermove", a, { passive: !1 }), this._domElement.ownerDocument.removeEventListener("mousemove", l), this._domElement.ownerDocument.removeEventListener("pointerup", h), this._domElement.ownerDocument.removeEventListener("mouseup", f), this.dispatchEvent({ type: "controlend" }));
    };
    this.lockPointer = () => {
      !this._enabled || !this._domElement || (this.cancel(), this._lockedPointer = {
        pointerId: -1,
        clientX: 0,
        clientY: 0,
        deltaX: 0,
        deltaY: 0,
        mouseButton: null
      }, this._activePointers.push(this._lockedPointer), this._domElement.ownerDocument.removeEventListener("pointermove", a, { passive: !1 }), this._domElement.ownerDocument.removeEventListener("pointerup", h), this._domElement.requestPointerLock(), this._domElement.ownerDocument.addEventListener("pointerlockchange", R), this._domElement.ownerDocument.addEventListener("pointerlockerror", S), this._domElement.ownerDocument.addEventListener("pointermove", a, { passive: !1 }), this._domElement.ownerDocument.addEventListener("pointerup", h), E());
    }, this.unlockPointer = () => {
      this._lockedPointer !== null && (this._disposePointer(this._lockedPointer), this._lockedPointer = null), document.exitPointerLock(), this.cancel(), this._domElement && (this._domElement.ownerDocument.removeEventListener("pointerlockchange", R), this._domElement.ownerDocument.removeEventListener("pointerlockerror", S));
    };
    const R = () => {
      this._domElement && this._domElement.ownerDocument.pointerLockElement === this._domElement || this.unlockPointer();
    }, S = () => {
      this.unlockPointer();
    };
    this._addAllEventListeners = (m) => {
      this._domElement = m, this._domElement.style.touchAction = "none", this._domElement.style.userSelect = "none", this._domElement.style.webkitUserSelect = "none", this._domElement.addEventListener("pointerdown", r), rc && this._domElement.addEventListener("mousedown", o), this._domElement.addEventListener("pointercancel", h), this._domElement.addEventListener("wheel", u, { passive: !1 }), this._domElement.addEventListener("contextmenu", d);
    }, this._removeAllEventListeners = () => {
      this._domElement && (this._domElement.style.touchAction = "", this._domElement.style.userSelect = "", this._domElement.style.webkitUserSelect = "", this._domElement.removeEventListener("pointerdown", r), this._domElement.removeEventListener("mousedown", o), this._domElement.removeEventListener("pointercancel", h), this._domElement.removeEventListener("wheel", u, { passive: !1 }), this._domElement.removeEventListener("contextmenu", d), this._domElement.ownerDocument.removeEventListener("pointermove", a, { passive: !1 }), this._domElement.ownerDocument.removeEventListener("mousemove", l), this._domElement.ownerDocument.removeEventListener("pointerup", h), this._domElement.ownerDocument.removeEventListener("mouseup", f), this._domElement.ownerDocument.removeEventListener("pointerlockchange", R), this._domElement.ownerDocument.removeEventListener("pointerlockerror", S));
    }, this.cancel = () => {
      this._state !== H.NONE && (this._state = H.NONE, this._activePointers.length = 0, p());
    }, t && this.connect(t), this.update(0);
  }
  /**
   * The camera to be controlled
   * @category Properties
   */
  get camera() {
    return this._camera;
  }
  set camera(i) {
    this._camera = i, this.updateCameraUp(), this._camera.updateProjectionMatrix(), this._updateNearPlaneCorners(), this._needsUpdate = !0;
  }
  /**
   * Whether or not the controls are enabled.
   * `false` to disable user dragging/touch-move, but all methods works.
   * @category Properties
   */
  get enabled() {
    return this._enabled;
  }
  set enabled(i) {
    this._enabled = i, this._domElement && (i ? (this._domElement.style.touchAction = "none", this._domElement.style.userSelect = "none", this._domElement.style.webkitUserSelect = "none") : (this.cancel(), this._domElement.style.touchAction = "", this._domElement.style.userSelect = "", this._domElement.style.webkitUserSelect = ""));
  }
  /**
   * Returns `true` if the controls are active updating.
   * readonly value.
   * @category Properties
   */
  get active() {
    return !this._hasRested;
  }
  /**
   * Getter for the current `ACTION`.
   * readonly value.
   * @category Properties
   */
  get currentAction() {
    return this._state;
  }
  /**
   * get/set Current distance.
   * @category Properties
   */
  get distance() {
    return this._spherical.radius;
  }
  set distance(i) {
    this._spherical.radius === i && this._sphericalEnd.radius === i || (this._spherical.radius = i, this._sphericalEnd.radius = i, this._needsUpdate = !0);
  }
  // horizontal angle
  /**
   * get/set the azimuth angle (horizontal) in radians.
   * Every 360 degrees turn is added to `.azimuthAngle` value, which is accumulative.
   * @category Properties
   */
  get azimuthAngle() {
    return this._spherical.theta;
  }
  set azimuthAngle(i) {
    this._spherical.theta === i && this._sphericalEnd.theta === i || (this._spherical.theta = i, this._sphericalEnd.theta = i, this._needsUpdate = !0);
  }
  // vertical angle
  /**
   * get/set the polar angle (vertical) in radians.
   * @category Properties
   */
  get polarAngle() {
    return this._spherical.phi;
  }
  set polarAngle(i) {
    this._spherical.phi === i && this._sphericalEnd.phi === i || (this._spherical.phi = i, this._sphericalEnd.phi = i, this._needsUpdate = !0);
  }
  /**
   * Whether camera position should be enclosed in the boundary or not.
   * @category Properties
   */
  get boundaryEnclosesCamera() {
    return this._boundaryEnclosesCamera;
  }
  set boundaryEnclosesCamera(i) {
    this._boundaryEnclosesCamera = i, this._needsUpdate = !0;
  }
  /**
   * Set drag-start, touches and wheel enable area in the domElement.
   * each values are between `0` and `1` inclusive, where `0` is left/top and `1` is right/bottom of the screen.
   * e.g. `{ x: 0, y: 0, width: 1, height: 1 }` for entire area.
   * @category Properties
   */
  set interactiveArea(i) {
    this._interactiveArea.width = Jt(i.width, 0, 1), this._interactiveArea.height = Jt(i.height, 0, 1), this._interactiveArea.x = Jt(i.x, 0, 1 - this._interactiveArea.width), this._interactiveArea.y = Jt(i.y, 0, 1 - this._interactiveArea.height);
  }
  /**
   * Adds the specified event listener.
   * Applicable event types (which is `K`) are:
   * | Event name          | Timing |
   * | ------------------- | ------ |
   * | `'controlstart'`    | When the user starts to control the camera via mouse / touches. ¹ |
   * | `'control'`         | When the user controls the camera (dragging). |
   * | `'controlend'`      | When the user ends to control the camera. ¹ |
   * | `'transitionstart'` | When any kind of transition starts, either user control or using a method with `enableTransition = true` |
   * | `'update'`          | When the camera position is updated. |
   * | `'wake'`            | When the camera starts moving. |
   * | `'rest'`            | When the camera movement is below `.restThreshold` ². |
   * | `'sleep'`           | When the camera end moving. |
   *
   * 1. `mouseButtons.wheel` (Mouse wheel control) does not emit `'controlstart'` and `'controlend'`. `mouseButtons.wheel` uses scroll-event internally, and scroll-event happens intermittently. That means "start" and "end" cannot be detected.
   * 2. Due to damping, `sleep` will usually fire a few seconds after the camera _appears_ to have stopped moving. If you want to do something (e.g. enable UI, perform another transition) at the point when the camera has stopped, you probably want the `rest` event. This can be fine tuned using the `.restThreshold` parameter. See the [Rest and Sleep Example](https://yomotsu.github.io/camera-controls/examples/rest-and-sleep.html).
   *
   * e.g.
   * ```
   * cameraControl.addEventListener( 'controlstart', myCallbackFunction );
   * ```
   * @param type event name
   * @param listener handler function
   * @category Methods
   */
  addEventListener(i, t) {
    super.addEventListener(i, t);
  }
  /**
   * Removes the specified event listener
   * e.g.
   * ```
   * cameraControl.addEventListener( 'controlstart', myCallbackFunction );
   * ```
   * @param type event name
   * @param listener handler function
   * @category Methods
   */
  removeEventListener(i, t) {
    super.removeEventListener(i, t);
  }
  /**
   * Rotate azimuthal angle(horizontal) and polar angle(vertical).
   * Every value is added to the current value.
   * @param azimuthAngle Azimuth rotate angle. In radian.
   * @param polarAngle Polar rotate angle. In radian.
   * @param enableTransition Whether to move smoothly or immediately
   * @category Methods
   */
  rotate(i, t, e = !1) {
    return this.rotateTo(this._sphericalEnd.theta + i, this._sphericalEnd.phi + t, e);
  }
  /**
   * Rotate azimuthal angle(horizontal) to the given angle and keep the same polar angle(vertical) target.
   *
   * e.g.
   * ```
   * cameraControls.rotateAzimuthTo( 30 * THREE.MathUtils.DEG2RAD, true );
   * ```
   * @param azimuthAngle Azimuth rotate angle. In radian.
   * @param enableTransition Whether to move smoothly or immediately
   * @category Methods
   */
  rotateAzimuthTo(i, t = !1) {
    return this.rotateTo(i, this._sphericalEnd.phi, t);
  }
  /**
   * Rotate polar angle(vertical) to the given angle and keep the same azimuthal angle(horizontal) target.
   *
   * e.g.
   * ```
   * cameraControls.rotatePolarTo( 30 * THREE.MathUtils.DEG2RAD, true );
   * ```
   * @param polarAngle Polar rotate angle. In radian.
   * @param enableTransition Whether to move smoothly or immediately
   * @category Methods
   */
  rotatePolarTo(i, t = !1) {
    return this.rotateTo(this._sphericalEnd.theta, i, t);
  }
  /**
   * Rotate azimuthal angle(horizontal) and polar angle(vertical) to the given angle.
   * Camera view will rotate over the orbit pivot absolutely:
   *
   * azimuthAngle
   * ```
   *       0º
   *         \
   * 90º -----+----- -90º
   *           \
   *           180º
   * ```
   * | direction | angle                  |
   * | --------- | ---------------------- |
   * | front     | 0º                     |
   * | left      | 90º (`Math.PI / 2`)    |
   * | right     | -90º (`- Math.PI / 2`) |
   * | back      | 180º (`Math.PI`)       |
   *
   * polarAngle
   * ```
   *     180º
   *      |
   *      90º
   *      |
   *      0º
   * ```
   * | direction            | angle                  |
   * | -------------------- | ---------------------- |
   * | top/sky              | 180º (`Math.PI`)       |
   * | horizontal from view | 90º (`Math.PI / 2`)    |
   * | bottom/floor         | 0º                     |
   *
   * @param azimuthAngle Azimuth rotate angle to. In radian.
   * @param polarAngle Polar rotate angle to. In radian.
   * @param enableTransition  Whether to move smoothly or immediately
   * @category Methods
   */
  rotateTo(i, t, e = !1) {
    this._isUserControllingRotate = !1;
    const s = Jt(i, this.minAzimuthAngle, this.maxAzimuthAngle), n = Jt(t, this.minPolarAngle, this.maxPolarAngle);
    this._sphericalEnd.theta = s, this._sphericalEnd.phi = n, this._sphericalEnd.makeSafe(), this._needsUpdate = !0, e || (this._spherical.theta = this._sphericalEnd.theta, this._spherical.phi = this._sphericalEnd.phi);
    const r = !e || pt(this._spherical.theta, this._sphericalEnd.theta, this.restThreshold) && pt(this._spherical.phi, this._sphericalEnd.phi, this.restThreshold);
    return this._createOnRestPromise(r);
  }
  /**
   * Dolly in/out camera position.
   * @param distance Distance of dollyIn. Negative number for dollyOut.
   * @param enableTransition Whether to move smoothly or immediately.
   * @category Methods
   */
  dolly(i, t = !1) {
    return this.dollyTo(this._sphericalEnd.radius - i, t);
  }
  /**
   * Dolly in/out camera position to given distance.
   * @param distance Distance of dolly.
   * @param enableTransition Whether to move smoothly or immediately.
   * @category Methods
   */
  dollyTo(i, t = !1) {
    return this._isUserControllingDolly = !1, this._lastDollyDirection = Ke.NONE, this._changedDolly = 0, this._dollyToNoClamp(Jt(i, this.minDistance, this.maxDistance), t);
  }
  _dollyToNoClamp(i, t = !1) {
    const e = this._sphericalEnd.radius;
    if (this.colliderMeshes.length >= 1) {
      const r = this._collisionTest(), o = pt(r, this._spherical.radius);
      if (!(e > i) && o)
        return Promise.resolve();
      this._sphericalEnd.radius = Math.min(i, r);
    } else
      this._sphericalEnd.radius = i;
    this._needsUpdate = !0, t || (this._spherical.radius = this._sphericalEnd.radius);
    const n = !t || pt(this._spherical.radius, this._sphericalEnd.radius, this.restThreshold);
    return this._createOnRestPromise(n);
  }
  /**
   * Dolly in, but does not change the distance between the target and the camera, and moves the target position instead.
   * Specify a negative value for dolly out.
   * @param distance Distance of dolly.
   * @param enableTransition Whether to move smoothly or immediately.
   * @category Methods
   */
  dollyInFixed(i, t = !1) {
    this._targetEnd.add(this._getCameraDirection(Fi).multiplyScalar(i)), t || this._target.copy(this._targetEnd);
    const e = !t || pt(this._target.x, this._targetEnd.x, this.restThreshold) && pt(this._target.y, this._targetEnd.y, this.restThreshold) && pt(this._target.z, this._targetEnd.z, this.restThreshold);
    return this._createOnRestPromise(e);
  }
  /**
   * Zoom in/out camera. The value is added to camera zoom.
   * Limits set with `.minZoom` and `.maxZoom`
   * @param zoomStep zoom scale
   * @param enableTransition Whether to move smoothly or immediately
   * @category Methods
   */
  zoom(i, t = !1) {
    return this.zoomTo(this._zoomEnd + i, t);
  }
  /**
   * Zoom in/out camera to given scale. The value overwrites camera zoom.
   * Limits set with .minZoom and .maxZoom
   * @param zoom
   * @param enableTransition
   * @category Methods
   */
  zoomTo(i, t = !1) {
    this._isUserControllingZoom = !1, this._zoomEnd = Jt(i, this.minZoom, this.maxZoom), this._needsUpdate = !0, t || (this._zoom = this._zoomEnd);
    const e = !t || pt(this._zoom, this._zoomEnd, this.restThreshold);
    return this._changedZoom = 0, this._createOnRestPromise(e);
  }
  /**
   * @deprecated `pan()` has been renamed to `truck()`
   * @category Methods
   */
  pan(i, t, e = !1) {
    return console.warn("`pan` has been renamed to `truck`"), this.truck(i, t, e);
  }
  /**
   * Truck and pedestal camera using current azimuthal angle
   * @param x Horizontal translate amount
   * @param y Vertical translate amount
   * @param enableTransition Whether to move smoothly or immediately
   * @category Methods
   */
  truck(i, t, e = !1) {
    this._camera.updateMatrix(), le.setFromMatrixColumn(this._camera.matrix, 0), he.setFromMatrixColumn(this._camera.matrix, 1), le.multiplyScalar(i), he.multiplyScalar(-t);
    const s = ut.copy(le).add(he), n = Et.copy(this._targetEnd).add(s);
    return this.moveTo(n.x, n.y, n.z, e);
  }
  /**
   * Move forward / backward.
   * @param distance Amount to move forward / backward. Negative value to move backward
   * @param enableTransition Whether to move smoothly or immediately
   * @category Methods
   */
  forward(i, t = !1) {
    ut.setFromMatrixColumn(this._camera.matrix, 0), ut.crossVectors(this._camera.up, ut), ut.multiplyScalar(i);
    const e = Et.copy(this._targetEnd).add(ut);
    return this.moveTo(e.x, e.y, e.z, t);
  }
  /**
   * Move up / down.
   * @param height Amount to move up / down. Negative value to move down
   * @param enableTransition Whether to move smoothly or immediately
   * @category Methods
   */
  elevate(i, t = !1) {
    return ut.copy(this._camera.up).multiplyScalar(i), this.moveTo(this._targetEnd.x + ut.x, this._targetEnd.y + ut.y, this._targetEnd.z + ut.z, t);
  }
  /**
   * Move target position to given point.
   * @param x x coord to move center position
   * @param y y coord to move center position
   * @param z z coord to move center position
   * @param enableTransition Whether to move smoothly or immediately
   * @category Methods
   */
  moveTo(i, t, e, s = !1) {
    this._isUserControllingTruck = !1;
    const n = ut.set(i, t, e).sub(this._targetEnd);
    this._encloseToBoundary(this._targetEnd, n, this.boundaryFriction), this._needsUpdate = !0, s || this._target.copy(this._targetEnd);
    const r = !s || pt(this._target.x, this._targetEnd.x, this.restThreshold) && pt(this._target.y, this._targetEnd.y, this.restThreshold) && pt(this._target.z, this._targetEnd.z, this.restThreshold);
    return this._createOnRestPromise(r);
  }
  /**
   * Look in the given point direction.
   * @param x point x.
   * @param y point y.
   * @param z point z.
   * @param enableTransition Whether to move smoothly or immediately.
   * @returns Transition end promise
   * @category Methods
   */
  lookInDirectionOf(i, t, e, s = !1) {
    const o = ut.set(i, t, e).sub(this._targetEnd).normalize().multiplyScalar(-this._sphericalEnd.radius);
    return this.setPosition(o.x, o.y, o.z, s);
  }
  /**
   * Fit the viewport to the box or the bounding box of the object, using the nearest axis. paddings are in unit.
   * set `cover: true` to fill enter screen.
   * e.g.
   * ```
   * cameraControls.fitToBox( myMesh );
   * ```
   * @param box3OrObject Axis aligned bounding box to fit the view.
   * @param enableTransition Whether to move smoothly or immediately.
   * @param options | `<object>` { cover: boolean, paddingTop: number, paddingLeft: number, paddingBottom: number, paddingRight: number }
   * @returns Transition end promise
   * @category Methods
   */
  fitToBox(i, t, { cover: e = !1, paddingLeft: s = 0, paddingRight: n = 0, paddingBottom: r = 0, paddingTop: o = 0 } = {}) {
    const a = [], l = i.isBox3 ? ei.copy(i) : ei.setFromObject(i);
    l.isEmpty() && (console.warn("camera-controls: fitTo() cannot be used with an empty box. Aborting"), Promise.resolve());
    const h = Rr(this._sphericalEnd.theta, mr), f = Rr(this._sphericalEnd.phi, mr);
    a.push(this.rotateTo(h, f, t));
    const I = ut.setFromSpherical(this._sphericalEnd).normalize(), u = Nr.setFromUnitVectors(I, cn), d = pt(Math.abs(I.y), 1);
    d && u.multiply(hn.setFromAxisAngle(os, h)), u.multiply(this._yAxisUpSpaceInverse);
    const E = Or.makeEmpty();
    Et.copy(l.min).applyQuaternion(u), E.expandByPoint(Et), Et.copy(l.min).setX(l.max.x).applyQuaternion(u), E.expandByPoint(Et), Et.copy(l.min).setY(l.max.y).applyQuaternion(u), E.expandByPoint(Et), Et.copy(l.max).setZ(l.min.z).applyQuaternion(u), E.expandByPoint(Et), Et.copy(l.min).setZ(l.max.z).applyQuaternion(u), E.expandByPoint(Et), Et.copy(l.max).setY(l.min.y).applyQuaternion(u), E.expandByPoint(Et), Et.copy(l.max).setX(l.min.x).applyQuaternion(u), E.expandByPoint(Et), Et.copy(l.max).applyQuaternion(u), E.expandByPoint(Et), E.min.x -= s, E.min.y -= r, E.max.x += n, E.max.y += o, u.setFromUnitVectors(cn, I), d && u.premultiply(hn.invert()), u.premultiply(this._yAxisUpSpace);
    const T = E.getSize(ut), p = E.getCenter(Et).applyQuaternion(u);
    if (ve(this._camera)) {
      const R = this.getDistanceToFitBox(T.x, T.y, T.z, e);
      a.push(this.moveTo(p.x, p.y, p.z, t)), a.push(this.dollyTo(R, t)), a.push(this.setFocalOffset(0, 0, 0, t));
    } else if (Ne(this._camera)) {
      const R = this._camera, S = R.right - R.left, m = R.top - R.bottom, F = e ? Math.max(S / T.x, m / T.y) : Math.min(S / T.x, m / T.y);
      a.push(this.moveTo(p.x, p.y, p.z, t)), a.push(this.zoomTo(F, t)), a.push(this.setFocalOffset(0, 0, 0, t));
    }
    return Promise.all(a);
  }
  /**
   * Fit the viewport to the sphere or the bounding sphere of the object.
   * @param sphereOrMesh
   * @param enableTransition
   * @category Methods
   */
  fitToSphere(i, t) {
    const e = [], n = i instanceof lt.Sphere ? ln.copy(i) : vt.createBoundingSphere(i, ln);
    if (e.push(this.moveTo(n.center.x, n.center.y, n.center.z, t)), ve(this._camera)) {
      const r = this.getDistanceToFitSphere(n.radius);
      e.push(this.dollyTo(r, t));
    } else if (Ne(this._camera)) {
      const r = this._camera.right - this._camera.left, o = this._camera.top - this._camera.bottom, a = 2 * n.radius, l = Math.min(r / a, o / a);
      e.push(this.zoomTo(l, t));
    }
    return e.push(this.setFocalOffset(0, 0, 0, t)), Promise.all(e);
  }
  /**
   * Look at the `target` from the `position`.
   * @param positionX
   * @param positionY
   * @param positionZ
   * @param targetX
   * @param targetY
   * @param targetZ
   * @param enableTransition
   * @category Methods
   */
  setLookAt(i, t, e, s, n, r, o = !1) {
    this._isUserControllingRotate = !1, this._isUserControllingDolly = !1, this._isUserControllingTruck = !1, this._lastDollyDirection = Ke.NONE, this._changedDolly = 0;
    const a = Et.set(s, n, r), l = ut.set(i, t, e);
    this._targetEnd.copy(a), this._sphericalEnd.setFromVector3(l.sub(a).applyQuaternion(this._yAxisUpSpace)), this.normalizeRotations(), this._needsUpdate = !0, o || (this._target.copy(this._targetEnd), this._spherical.copy(this._sphericalEnd));
    const h = !o || pt(this._target.x, this._targetEnd.x, this.restThreshold) && pt(this._target.y, this._targetEnd.y, this.restThreshold) && pt(this._target.z, this._targetEnd.z, this.restThreshold) && pt(this._spherical.theta, this._sphericalEnd.theta, this.restThreshold) && pt(this._spherical.phi, this._sphericalEnd.phi, this.restThreshold) && pt(this._spherical.radius, this._sphericalEnd.radius, this.restThreshold);
    return this._createOnRestPromise(h);
  }
  /**
   * Similar to setLookAt, but it interpolates between two states.
   * @param positionAX
   * @param positionAY
   * @param positionAZ
   * @param targetAX
   * @param targetAY
   * @param targetAZ
   * @param positionBX
   * @param positionBY
   * @param positionBZ
   * @param targetBX
   * @param targetBY
   * @param targetBZ
   * @param t
   * @param enableTransition
   * @category Methods
   */
  lerpLookAt(i, t, e, s, n, r, o, a, l, h, f, I, u, d = !1) {
    this._isUserControllingRotate = !1, this._isUserControllingDolly = !1, this._isUserControllingTruck = !1, this._lastDollyDirection = Ke.NONE, this._changedDolly = 0;
    const E = ut.set(s, n, r), T = Et.set(i, t, e);
    Xt.setFromVector3(T.sub(E).applyQuaternion(this._yAxisUpSpace));
    const p = ti.set(h, f, I), R = Et.set(o, a, l);
    Si.setFromVector3(R.sub(p).applyQuaternion(this._yAxisUpSpace)), this._targetEnd.copy(E.lerp(p, u));
    const S = Si.theta - Xt.theta, m = Si.phi - Xt.phi, F = Si.radius - Xt.radius;
    this._sphericalEnd.set(Xt.radius + F * u, Xt.phi + m * u, Xt.theta + S * u), this.normalizeRotations(), this._needsUpdate = !0, d || (this._target.copy(this._targetEnd), this._spherical.copy(this._sphericalEnd));
    const O = !d || pt(this._target.x, this._targetEnd.x, this.restThreshold) && pt(this._target.y, this._targetEnd.y, this.restThreshold) && pt(this._target.z, this._targetEnd.z, this.restThreshold) && pt(this._spherical.theta, this._sphericalEnd.theta, this.restThreshold) && pt(this._spherical.phi, this._sphericalEnd.phi, this.restThreshold) && pt(this._spherical.radius, this._sphericalEnd.radius, this.restThreshold);
    return this._createOnRestPromise(O);
  }
  /**
   * Set angle and distance by given position.
   * An alias of `setLookAt()`, without target change. Thus keep gazing at the current target
   * @param positionX
   * @param positionY
   * @param positionZ
   * @param enableTransition
   * @category Methods
   */
  setPosition(i, t, e, s = !1) {
    return this.setLookAt(i, t, e, this._targetEnd.x, this._targetEnd.y, this._targetEnd.z, s);
  }
  /**
   * Set the target position where gaze at.
   * An alias of `setLookAt()`, without position change. Thus keep the same position.
   * @param targetX
   * @param targetY
   * @param targetZ
   * @param enableTransition
   * @category Methods
   */
  setTarget(i, t, e, s = !1) {
    const n = this.getPosition(ut), r = this.setLookAt(n.x, n.y, n.z, i, t, e, s);
    return this._sphericalEnd.phi = Jt(this._sphericalEnd.phi, this.minPolarAngle, this.maxPolarAngle), r;
  }
  /**
   * Set focal offset using the screen parallel coordinates. z doesn't affect in Orthographic as with Dolly.
   * @param x
   * @param y
   * @param z
   * @param enableTransition
   * @category Methods
   */
  setFocalOffset(i, t, e, s = !1) {
    this._isUserControllingOffset = !1, this._focalOffsetEnd.set(i, t, e), this._needsUpdate = !0, s || this._focalOffset.copy(this._focalOffsetEnd);
    const n = !s || pt(this._focalOffset.x, this._focalOffsetEnd.x, this.restThreshold) && pt(this._focalOffset.y, this._focalOffsetEnd.y, this.restThreshold) && pt(this._focalOffset.z, this._focalOffsetEnd.z, this.restThreshold);
    return this._createOnRestPromise(n);
  }
  /**
   * Set orbit point without moving the camera.
   * SHOULD NOT RUN DURING ANIMATIONS. `setOrbitPoint()` will immediately fix the positions.
   * @param targetX
   * @param targetY
   * @param targetZ
   * @category Methods
   */
  setOrbitPoint(i, t, e) {
    this._camera.updateMatrixWorld(), le.setFromMatrixColumn(this._camera.matrixWorldInverse, 0), he.setFromMatrixColumn(this._camera.matrixWorldInverse, 1), Ue.setFromMatrixColumn(this._camera.matrixWorldInverse, 2);
    const s = ut.set(i, t, e), n = s.distanceTo(this._camera.position), r = s.sub(this._camera.position);
    le.multiplyScalar(r.x), he.multiplyScalar(r.y), Ue.multiplyScalar(r.z), ut.copy(le).add(he).add(Ue), ut.z = ut.z + n, this.dollyTo(n, !1), this.setFocalOffset(-ut.x, ut.y, -ut.z, !1), this.moveTo(i, t, e, !1);
  }
  /**
   * Set the boundary box that encloses the target of the camera. box3 is in THREE.Box3
   * @param box3
   * @category Methods
   */
  setBoundary(i) {
    if (!i) {
      this._boundary.min.set(-1 / 0, -1 / 0, -1 / 0), this._boundary.max.set(1 / 0, 1 / 0, 1 / 0), this._needsUpdate = !0;
      return;
    }
    this._boundary.copy(i), this._boundary.clampPoint(this._targetEnd, this._targetEnd), this._needsUpdate = !0;
  }
  /**
   * Set (or unset) the current viewport.
   * Set this when you want to use renderer viewport and .dollyToCursor feature at the same time.
   * @param viewportOrX
   * @param y
   * @param width
   * @param height
   * @category Methods
   */
  setViewport(i, t, e, s) {
    if (i === null) {
      this._viewport = null;
      return;
    }
    this._viewport = this._viewport || new lt.Vector4(), typeof i == "number" ? this._viewport.set(i, t, e, s) : this._viewport.copy(i);
  }
  /**
   * Calculate the distance to fit the box.
   * @param width box width
   * @param height box height
   * @param depth box depth
   * @returns distance
   * @category Methods
   */
  getDistanceToFitBox(i, t, e, s = !1) {
    if (an(this._camera, "getDistanceToFitBox"))
      return this._spherical.radius;
    const n = i / t, r = this._camera.getEffectiveFOV() * Ri, o = this._camera.aspect;
    return ((s ? n > o : n < o) ? t : i / o) * 0.5 / Math.tan(r * 0.5) + e * 0.5;
  }
  /**
   * Calculate the distance to fit the sphere.
   * @param radius sphere radius
   * @returns distance
   * @category Methods
   */
  getDistanceToFitSphere(i) {
    if (an(this._camera, "getDistanceToFitSphere"))
      return this._spherical.radius;
    const t = this._camera.getEffectiveFOV() * Ri, e = Math.atan(Math.tan(t * 0.5) * this._camera.aspect) * 2, s = 1 < this._camera.aspect ? t : e;
    return i / Math.sin(s * 0.5);
  }
  /**
   * Returns the orbit center position, where the camera looking at.
   * @param out The receiving Vector3 instance to copy the result
   * @param receiveEndValue Whether receive the transition end coords or current. default is `true`
   * @category Methods
   */
  getTarget(i, t = !0) {
    return (i && i.isVector3 ? i : new lt.Vector3()).copy(t ? this._targetEnd : this._target);
  }
  /**
   * Returns the camera position.
   * @param out The receiving Vector3 instance to copy the result
   * @param receiveEndValue Whether receive the transition end coords or current. default is `true`
   * @category Methods
   */
  getPosition(i, t = !0) {
    return (i && i.isVector3 ? i : new lt.Vector3()).setFromSpherical(t ? this._sphericalEnd : this._spherical).applyQuaternion(this._yAxisUpSpaceInverse).add(t ? this._targetEnd : this._target);
  }
  /**
   * Returns the spherical coordinates of the orbit.
   * @param out The receiving Spherical instance to copy the result
   * @param receiveEndValue Whether receive the transition end coords or current. default is `true`
   * @category Methods
   */
  getSpherical(i, t = !0) {
    return (i && i instanceof lt.Spherical ? i : new lt.Spherical()).copy(t ? this._sphericalEnd : this._spherical);
  }
  /**
   * Returns the focal offset, which is how much the camera appears to be translated in screen parallel coordinates.
   * @param out The receiving Vector3 instance to copy the result
   * @param receiveEndValue Whether receive the transition end coords or current. default is `true`
   * @category Methods
   */
  getFocalOffset(i, t = !0) {
    return (i && i.isVector3 ? i : new lt.Vector3()).copy(t ? this._focalOffsetEnd : this._focalOffset);
  }
  /**
   * Normalize camera azimuth angle rotation between 0 and 360 degrees.
   * @category Methods
   */
  normalizeRotations() {
    this._sphericalEnd.theta = this._sphericalEnd.theta % Je, this._sphericalEnd.theta < 0 && (this._sphericalEnd.theta += Je), this._spherical.theta += Je * Math.round((this._sphericalEnd.theta - this._spherical.theta) / Je);
  }
  /**
   * Reset all rotation and position to defaults.
   * @param enableTransition
   * @category Methods
   */
  reset(i = !1) {
    if (!pt(this._camera.up.x, this._cameraUp0.x) || !pt(this._camera.up.y, this._cameraUp0.y) || !pt(this._camera.up.z, this._cameraUp0.z)) {
      this._camera.up.copy(this._cameraUp0);
      const e = this.getPosition(ut);
      this.updateCameraUp(), this.setPosition(e.x, e.y, e.z);
    }
    const t = [
      this.setLookAt(this._position0.x, this._position0.y, this._position0.z, this._target0.x, this._target0.y, this._target0.z, i),
      this.setFocalOffset(this._focalOffset0.x, this._focalOffset0.y, this._focalOffset0.z, i),
      this.zoomTo(this._zoom0, i)
    ];
    return Promise.all(t);
  }
  /**
   * Set current camera position as the default position.
   * @category Methods
   */
  saveState() {
    this._cameraUp0.copy(this._camera.up), this.getTarget(this._target0), this.getPosition(this._position0), this._zoom0 = this._zoom, this._focalOffset0.copy(this._focalOffset);
  }
  /**
   * Sync camera-up direction.
   * When camera-up vector is changed, `.updateCameraUp()` must be called.
   * @category Methods
   */
  updateCameraUp() {
    this._yAxisUpSpace.setFromUnitVectors(this._camera.up, os), this._yAxisUpSpaceInverse.copy(this._yAxisUpSpace).invert();
  }
  /**
   * Apply current camera-up direction to the camera.
   * The orbit system will be re-initialized with the current position.
   * @category Methods
   */
  applyCameraUp() {
    const i = ut.subVectors(this._target, this._camera.position).normalize(), t = Et.crossVectors(i, this._camera.up);
    this._camera.up.crossVectors(t, i).normalize(), this._camera.updateMatrixWorld();
    const e = this.getPosition(ut);
    this.updateCameraUp(), this.setPosition(e.x, e.y, e.z);
  }
  /**
   * Update camera position and directions.
   * This should be called in your tick loop every time, and returns true if re-rendering is needed.
   * @param delta
   * @returns updated
   * @category Methods
   */
  update(i) {
    const t = this._sphericalEnd.theta - this._spherical.theta, e = this._sphericalEnd.phi - this._spherical.phi, s = this._sphericalEnd.radius - this._spherical.radius, n = Fr.subVectors(this._targetEnd, this._target), r = Sr.subVectors(this._focalOffsetEnd, this._focalOffset), o = this._zoomEnd - this._zoom;
    if (gt(t))
      this._thetaVelocity.value = 0, this._spherical.theta = this._sphericalEnd.theta;
    else {
      const f = this._isUserControllingRotate ? this.draggingSmoothTime : this.smoothTime;
      this._spherical.theta = ns(this._spherical.theta, this._sphericalEnd.theta, this._thetaVelocity, f, 1 / 0, i), this._needsUpdate = !0;
    }
    if (gt(e))
      this._phiVelocity.value = 0, this._spherical.phi = this._sphericalEnd.phi;
    else {
      const f = this._isUserControllingRotate ? this.draggingSmoothTime : this.smoothTime;
      this._spherical.phi = ns(this._spherical.phi, this._sphericalEnd.phi, this._phiVelocity, f, 1 / 0, i), this._needsUpdate = !0;
    }
    if (gt(s))
      this._radiusVelocity.value = 0, this._spherical.radius = this._sphericalEnd.radius;
    else {
      const f = this._isUserControllingDolly ? this.draggingSmoothTime : this.smoothTime;
      this._spherical.radius = ns(this._spherical.radius, this._sphericalEnd.radius, this._radiusVelocity, f, this.maxSpeed, i), this._needsUpdate = !0;
    }
    if (gt(n.x) && gt(n.y) && gt(n.z))
      this._targetVelocity.set(0, 0, 0), this._target.copy(this._targetEnd);
    else {
      const f = this._isUserControllingTruck ? this.draggingSmoothTime : this.smoothTime;
      gr(this._target, this._targetEnd, this._targetVelocity, f, this.maxSpeed, i, this._target), this._needsUpdate = !0;
    }
    if (gt(r.x) && gt(r.y) && gt(r.z))
      this._focalOffsetVelocity.set(0, 0, 0), this._focalOffset.copy(this._focalOffsetEnd);
    else {
      const f = this._isUserControllingOffset ? this.draggingSmoothTime : this.smoothTime;
      gr(this._focalOffset, this._focalOffsetEnd, this._focalOffsetVelocity, f, this.maxSpeed, i, this._focalOffset), this._needsUpdate = !0;
    }
    if (gt(o))
      this._zoomVelocity.value = 0, this._zoom = this._zoomEnd;
    else {
      const f = this._isUserControllingZoom ? this.draggingSmoothTime : this.smoothTime;
      this._zoom = ns(this._zoom, this._zoomEnd, this._zoomVelocity, f, 1 / 0, i);
    }
    if (this.dollyToCursor) {
      if (ve(this._camera) && this._changedDolly !== 0) {
        const f = this._spherical.radius - this._lastDistance, I = this._camera, u = this._getCameraDirection(Fi), d = ut.copy(u).cross(I.up).normalize();
        d.lengthSq() === 0 && (d.x = 1);
        const E = Et.crossVectors(d, u), T = this._sphericalEnd.radius * Math.tan(I.getEffectiveFOV() * Ri * 0.5), R = (this._sphericalEnd.radius - f - this._sphericalEnd.radius) / this._sphericalEnd.radius, S = ti.copy(this._targetEnd).add(d.multiplyScalar(this._dollyControlCoord.x * T * I.aspect)).add(E.multiplyScalar(this._dollyControlCoord.y * T)), m = ut.copy(this._targetEnd).lerp(S, R), F = this._lastDollyDirection === Ke.IN && this._spherical.radius <= this.minDistance, O = this._lastDollyDirection === Ke.OUT && this.maxDistance <= this._spherical.radius;
        if (this.infinityDolly && (F || O)) {
          this._sphericalEnd.radius -= f, this._spherical.radius -= f;
          const w = Et.copy(u).multiplyScalar(-f);
          m.add(w);
        }
        this._boundary.clampPoint(m, m);
        const y = Et.subVectors(m, this._targetEnd);
        this._targetEnd.copy(m), this._target.add(y), this._changedDolly -= f, gt(this._changedDolly) && (this._changedDolly = 0);
      } else if (Ne(this._camera) && this._changedZoom !== 0) {
        const f = this._zoom - this._lastZoom, I = this._camera, u = ut.set(this._dollyControlCoord.x, this._dollyControlCoord.y, (I.near + I.far) / (I.near - I.far)).unproject(I), d = Et.set(0, 0, -1).applyQuaternion(I.quaternion), E = ti.copy(u).add(d.multiplyScalar(-u.dot(I.up))), p = -(this._zoom - f - this._zoom) / this._zoom, R = this._getCameraDirection(Fi), S = this._targetEnd.dot(R), m = ut.copy(this._targetEnd).lerp(E, p), F = m.dot(R), O = R.multiplyScalar(F - S);
        m.sub(O), this._boundary.clampPoint(m, m);
        const y = Et.subVectors(m, this._targetEnd);
        this._targetEnd.copy(m), this._target.add(y), this._changedZoom -= f, gt(this._changedZoom) && (this._changedZoom = 0);
      }
    }
    this._camera.zoom !== this._zoom && (this._camera.zoom = this._zoom, this._camera.updateProjectionMatrix(), this._updateNearPlaneCorners(), this._needsUpdate = !0), this._dragNeedsUpdate = !0;
    const a = this._collisionTest();
    this._spherical.radius = Math.min(this._spherical.radius, a), this._spherical.makeSafe(), this._camera.position.setFromSpherical(this._spherical).applyQuaternion(this._yAxisUpSpaceInverse).add(this._target), this._camera.lookAt(this._target), (!gt(this._focalOffset.x) || !gt(this._focalOffset.y) || !gt(this._focalOffset.z)) && (this._camera.updateMatrixWorld(), le.setFromMatrixColumn(this._camera.matrix, 0), he.setFromMatrixColumn(this._camera.matrix, 1), Ue.setFromMatrixColumn(this._camera.matrix, 2), le.multiplyScalar(this._focalOffset.x), he.multiplyScalar(-this._focalOffset.y), Ue.multiplyScalar(this._focalOffset.z), ut.copy(le).add(he).add(Ue), this._camera.position.add(ut)), this._boundaryEnclosesCamera && this._encloseToBoundary(this._camera.position.copy(this._target), ut.setFromSpherical(this._spherical).applyQuaternion(this._yAxisUpSpaceInverse), 1);
    const h = this._needsUpdate;
    return h && !this._updatedLastTime ? (this._hasRested = !1, this.dispatchEvent({ type: "wake" }), this.dispatchEvent({ type: "update" })) : h ? (this.dispatchEvent({ type: "update" }), gt(t, this.restThreshold) && gt(e, this.restThreshold) && gt(s, this.restThreshold) && gt(n.x, this.restThreshold) && gt(n.y, this.restThreshold) && gt(n.z, this.restThreshold) && gt(r.x, this.restThreshold) && gt(r.y, this.restThreshold) && gt(r.z, this.restThreshold) && gt(o, this.restThreshold) && !this._hasRested && (this._hasRested = !0, this.dispatchEvent({ type: "rest" }))) : !h && this._updatedLastTime && this.dispatchEvent({ type: "sleep" }), this._lastDistance = this._spherical.radius, this._lastZoom = this._zoom, this._updatedLastTime = h, this._needsUpdate = !1, h;
  }
  /**
   * Get all state in JSON string
   * @category Methods
   */
  toJSON() {
    return JSON.stringify({
      enabled: this._enabled,
      minDistance: this.minDistance,
      maxDistance: gi(this.maxDistance),
      minZoom: this.minZoom,
      maxZoom: gi(this.maxZoom),
      minPolarAngle: this.minPolarAngle,
      maxPolarAngle: gi(this.maxPolarAngle),
      minAzimuthAngle: gi(this.minAzimuthAngle),
      maxAzimuthAngle: gi(this.maxAzimuthAngle),
      smoothTime: this.smoothTime,
      draggingSmoothTime: this.draggingSmoothTime,
      dollySpeed: this.dollySpeed,
      truckSpeed: this.truckSpeed,
      dollyToCursor: this.dollyToCursor,
      verticalDragToForward: this.verticalDragToForward,
      target: this._targetEnd.toArray(),
      position: ut.setFromSpherical(this._sphericalEnd).add(this._targetEnd).toArray(),
      zoom: this._zoomEnd,
      focalOffset: this._focalOffsetEnd.toArray(),
      target0: this._target0.toArray(),
      position0: this._position0.toArray(),
      zoom0: this._zoom0,
      focalOffset0: this._focalOffset0.toArray()
    });
  }
  /**
   * Reproduce the control state with JSON. enableTransition is where anim or not in a boolean.
   * @param json
   * @param enableTransition
   * @category Methods
   */
  fromJSON(i, t = !1) {
    const e = JSON.parse(i);
    this.enabled = e.enabled, this.minDistance = e.minDistance, this.maxDistance = Ai(e.maxDistance), this.minZoom = e.minZoom, this.maxZoom = Ai(e.maxZoom), this.minPolarAngle = e.minPolarAngle, this.maxPolarAngle = Ai(e.maxPolarAngle), this.minAzimuthAngle = Ai(e.minAzimuthAngle), this.maxAzimuthAngle = Ai(e.maxAzimuthAngle), this.smoothTime = e.smoothTime, this.draggingSmoothTime = e.draggingSmoothTime, this.dollySpeed = e.dollySpeed, this.truckSpeed = e.truckSpeed, this.dollyToCursor = e.dollyToCursor, this.verticalDragToForward = e.verticalDragToForward, this._target0.fromArray(e.target0), this._position0.fromArray(e.position0), this._zoom0 = e.zoom0, this._focalOffset0.fromArray(e.focalOffset0), this.moveTo(e.target[0], e.target[1], e.target[2], t), Xt.setFromVector3(ut.fromArray(e.position).sub(this._targetEnd).applyQuaternion(this._yAxisUpSpace)), this.rotateTo(Xt.theta, Xt.phi, t), this.dollyTo(Xt.radius, t), this.zoomTo(e.zoom, t), this.setFocalOffset(e.focalOffset[0], e.focalOffset[1], e.focalOffset[2], t), this._needsUpdate = !0;
  }
  /**
   * Attach all internal event handlers to enable drag control.
   * @category Methods
   */
  connect(i) {
    if (this._domElement) {
      console.warn("camera-controls is already connected.");
      return;
    }
    i.setAttribute("data-camera-controls-version", sc), this._addAllEventListeners(i), this._getClientRect(this._elementRect);
  }
  /**
   * Detach all internal event handlers to disable drag control.
   */
  disconnect() {
    this.cancel(), this._removeAllEventListeners(), this._domElement && (this._domElement.removeAttribute("data-camera-controls-version"), this._domElement = void 0);
  }
  /**
   * Dispose the cameraControls instance itself, remove all eventListeners.
   * @category Methods
   */
  dispose() {
    this.removeAllEventListeners(), this.disconnect();
  }
  // it's okay to expose public though
  _getTargetDirection(i) {
    return i.setFromSpherical(this._spherical).divideScalar(this._spherical.radius).applyQuaternion(this._yAxisUpSpaceInverse);
  }
  // it's okay to expose public though
  _getCameraDirection(i) {
    return this._getTargetDirection(i).negate();
  }
  _findPointerById(i) {
    return this._activePointers.find((t) => t.pointerId === i);
  }
  _findPointerByMouseButton(i) {
    return this._activePointers.find((t) => t.mouseButton === i);
  }
  _disposePointer(i) {
    this._activePointers.splice(this._activePointers.indexOf(i), 1);
  }
  _encloseToBoundary(i, t, e) {
    const s = t.lengthSq();
    if (s === 0)
      return i;
    const n = Et.copy(t).add(i), o = this._boundary.clampPoint(n, ti).sub(n), a = o.lengthSq();
    if (a === 0)
      return i.add(t);
    if (a === s)
      return i;
    if (e === 0)
      return i.add(t).add(o);
    {
      const l = 1 + e * a / t.dot(o);
      return i.add(Et.copy(t).multiplyScalar(l)).add(o.multiplyScalar(1 - e));
    }
  }
  _updateNearPlaneCorners() {
    if (ve(this._camera)) {
      const i = this._camera, t = i.near, e = i.getEffectiveFOV() * Ri, s = Math.tan(e * 0.5) * t, n = s * i.aspect;
      this._nearPlaneCorners[0].set(-n, -s, 0), this._nearPlaneCorners[1].set(n, -s, 0), this._nearPlaneCorners[2].set(n, s, 0), this._nearPlaneCorners[3].set(-n, s, 0);
    } else if (Ne(this._camera)) {
      const i = this._camera, t = 1 / i.zoom, e = i.left * t, s = i.right * t, n = i.top * t, r = i.bottom * t;
      this._nearPlaneCorners[0].set(e, n, 0), this._nearPlaneCorners[1].set(s, n, 0), this._nearPlaneCorners[2].set(s, r, 0), this._nearPlaneCorners[3].set(e, r, 0);
    }
  }
  // lateUpdate
  _collisionTest() {
    let i = 1 / 0;
    if (!(this.colliderMeshes.length >= 1) || an(this._camera, "_collisionTest"))
      return i;
    const e = this._getTargetDirection(Fi);
    un.lookAt(Ar, e, this._camera.up);
    for (let s = 0; s < 4; s++) {
      const n = Et.copy(this._nearPlaneCorners[s]);
      n.applyMatrix4(un);
      const r = ti.addVectors(this._target, n);
      as.set(r, e), as.far = this._spherical.radius + 1;
      const o = as.intersectObjects(this.colliderMeshes);
      o.length !== 0 && o[0].distance < i && (i = o[0].distance);
    }
    return i;
  }
  /**
   * Get its client rect and package into given `DOMRect` .
   */
  _getClientRect(i) {
    if (!this._domElement)
      return;
    const t = this._domElement.getBoundingClientRect();
    return i.x = t.left, i.y = t.top, this._viewport ? (i.x += this._viewport.x, i.y += t.height - this._viewport.w - this._viewport.y, i.width = this._viewport.z, i.height = this._viewport.w) : (i.width = t.width, i.height = t.height), i;
  }
  _createOnRestPromise(i) {
    return i ? Promise.resolve() : (this._hasRested = !1, this.dispatchEvent({ type: "transitionstart" }), new Promise((t) => {
      const e = () => {
        this.removeEventListener("rest", e), t();
      };
      this.addEventListener("rest", e);
    }));
  }
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  _addAllEventListeners(i) {
  }
  _removeAllEventListeners() {
  }
  /**
   * backward compatible
   * @deprecated use smoothTime (in seconds) instead
   * @category Properties
   */
  get dampingFactor() {
    return console.warn(".dampingFactor has been deprecated. use smoothTime (in seconds) instead."), 0;
  }
  /**
   * backward compatible
   * @deprecated use smoothTime (in seconds) instead
   * @category Properties
   */
  set dampingFactor(i) {
    console.warn(".dampingFactor has been deprecated. use smoothTime (in seconds) instead.");
  }
  /**
   * backward compatible
   * @deprecated use draggingSmoothTime (in seconds) instead
   * @category Properties
   */
  get draggingDampingFactor() {
    return console.warn(".draggingDampingFactor has been deprecated. use draggingSmoothTime (in seconds) instead."), 0;
  }
  /**
   * backward compatible
   * @deprecated use draggingSmoothTime (in seconds) instead
   * @category Properties
   */
  set draggingDampingFactor(i) {
    console.warn(".draggingDampingFactor has been deprecated. use draggingSmoothTime (in seconds) instead.");
  }
  static createBoundingSphere(i, t = new lt.Sphere()) {
    const e = t, s = e.center;
    ei.makeEmpty(), i.traverseVisible((r) => {
      r.isMesh && ei.expandByObject(r);
    }), ei.getCenter(s);
    let n = 0;
    return i.traverseVisible((r) => {
      if (!r.isMesh)
        return;
      const o = r, a = o.geometry.clone();
      a.applyMatrix4(o.matrixWorld);
      const h = a.attributes.position;
      for (let f = 0, I = h.count; f < I; f++)
        ut.fromBufferAttribute(h, f), n = Math.max(n, s.distanceToSquared(ut));
    }), e.radius = Math.sqrt(n), e;
  }
}
class xi extends Va {
  constructor(t) {
    super(t);
    /** {@link Updateable.onBeforeUpdate} */
    C(this, "onBeforeUpdate", new j());
    /** {@link Updateable.onAfterUpdate} */
    C(this, "onAfterUpdate", new j());
    /**
     * Event that is triggered when the aspect of the camera has been updated.
     * This event is useful when you need to perform actions after the aspect of the camera has been changed.
     */
    C(this, "onAspectUpdated", new j());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * A three.js PerspectiveCamera or OrthographicCamera instance.
     * This camera is used for rendering the scene.
     */
    C(this, "three");
    C(this, "_allControls", /* @__PURE__ */ new Map());
    /**
     * Updates the aspect of the camera to match the size of the
     * {@link Components.renderer}.
     */
    C(this, "updateAspect", () => {
      var t;
      if (!(!this.currentWorld || !this.currentWorld.renderer)) {
        if (this.three instanceof D.OrthographicCamera) {
          this.onAspectUpdated.trigger();
          return;
        }
        if ((t = this.currentWorld.renderer) != null && t.isResizeable()) {
          const e = this.currentWorld.renderer.getSize();
          this.three.aspect = e.width / e.height, this.three.updateProjectionMatrix(), this.onAspectUpdated.trigger();
        }
      }
    });
    this.three = this.setupCamera(), this.setupEvents(!0), this.onWorldChanged.add(({ action: e, world: s }) => {
      if (e === "added") {
        const n = this.newCameraControls();
        this._allControls.set(s.uuid, n);
      }
      if (e === "removed") {
        const n = this._allControls.get(s.uuid);
        n && (n.dispose(), this._allControls.delete(s.uuid));
      }
    });
  }
  /**
   * The object that controls the camera. An instance of
   * [yomotsu's cameracontrols](https://github.com/yomotsu/camera-controls).
   * Transforming the camera directly will have no effect: you need to use this
   * object to move, rotate, look at objects, etc.
   */
  get controls() {
    if (!this.currentWorld)
      throw new Error("This camera needs a world to work!");
    const t = this._allControls.get(this.currentWorld.uuid);
    if (!t)
      throw new Error("Controls not found!");
    return t;
  }
  /**
   * Getter for the enabled state of the camera controls.
   * If the current world is null, it returns false.
   * Otherwise, it returns the enabled state of the camera controls.
   *
   * @returns {boolean} The enabled state of the camera controls.
   */
  get enabled() {
    return this.currentWorld === null ? !1 : this.controls.enabled;
  }
  /**
   * Setter for the enabled state of the camera controls.
   * If the current world is not null, it sets the enabled state of the camera controls to the provided value.
   *
   * @param {boolean} enabled - The new enabled state of the camera controls.
   */
  set enabled(t) {
    this.currentWorld !== null && (this.controls.enabled = t);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.setupEvents(!1), this.onAspectUpdated.reset(), this.onBeforeUpdate.reset(), this.onAfterUpdate.reset(), this.three.removeFromParent(), this.onDisposed.trigger(), this.onDisposed.reset();
    for (const [t, e] of this._allControls)
      e.dispose();
  }
  /** {@link Updateable.update} */
  update(t) {
    this.enabled && (this.onBeforeUpdate.trigger(this), this.controls.update(t), this.onAfterUpdate.trigger(this));
  }
  setupCamera() {
    const t = window.innerWidth / window.innerHeight, e = new D.PerspectiveCamera(60, t, 1, 1e3);
    return e.position.set(50, 50, 50), e.lookAt(new D.Vector3(0, 0, 0)), e;
  }
  newCameraControls() {
    if (!this.currentWorld)
      throw new Error("This camera needs a world to work!");
    if (!this.currentWorld.renderer)
      throw new Error("This camera needs a renderer to work!");
    vt.install({ THREE: xi.getSubsetOfThree() });
    const { domElement: t } = this.currentWorld.renderer.three, e = new vt(this.three, t);
    return e.smoothTime = 0.2, e.dollyToCursor = !0, e.infinityDolly = !0, e.minDistance = 6, e;
  }
  setupEvents(t) {
    t ? window.addEventListener("resize", this.updateAspect) : window.removeEventListener("resize", this.updateAspect);
  }
  static getSubsetOfThree() {
    return {
      MOUSE: D.MOUSE,
      Vector2: D.Vector2,
      Vector3: D.Vector3,
      Vector4: D.Vector4,
      Quaternion: D.Quaternion,
      Matrix4: D.Matrix4,
      Spherical: D.Spherical,
      Box3: D.Box3,
      Sphere: D.Sphere,
      Raycaster: D.Raycaster,
      MathUtils: D.MathUtils
    };
  }
}
const Os = class Os extends At {
  constructor(t) {
    super(t);
    /** {@link Updateable.onAfterUpdate} */
    C(this, "onAfterUpdate", new j());
    /** {@link Updateable.onBeforeUpdate} */
    C(this, "onBeforeUpdate", new j());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * A collection of worlds managed by this component.
     * The key is the unique identifier (UUID) of the world, and the value is the World instance.
     */
    C(this, "list", new re());
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    t.add(Os.uuid, this);
  }
  /**
   * Creates a new instance of a SimpleWorld and adds it to the list of worlds.
   *
   * @template T - The type of the scene, extending from BaseScene. Defaults to BaseScene.
   * @template U - The type of the camera, extending from BaseCamera. Defaults to BaseCamera.
   * @template S - The type of the renderer, extending from BaseRenderer. Defaults to BaseRenderer.
   *
   * @throws {Error} - Throws an error if a world with the same UUID already exists in the list.
   */
  create() {
    const t = new Qa(this.components), e = t.uuid;
    if (this.list.has(e))
      throw new Error("There is already a world with this name!");
    return this.list.set(e, t), t;
  }
  /**
   * Deletes a world from the list of worlds.
   *
   * @param {World} world - The world to be deleted.
   *
   * @throws {Error} - Throws an error if the provided world is not found in the list.
   */
  delete(t) {
    if (!this.list.has(t.uuid))
      throw new Error("The provided world is not found in the list!");
    this.list.delete(t.uuid), t.dispose();
  }
  /**
   * Disposes of the Worlds component and all its managed worlds.
   * This method sets the enabled flag to false, disposes of all worlds, clears the list,
   * and triggers the onDisposed event.
   */
  dispose() {
    this.enabled = !1;
    for (const [t, e] of this.list)
      e.dispose();
    this.list.clear(), this.onDisposed.trigger();
  }
  /** {@link Updateable.update} */
  update(t) {
    if (this.enabled)
      for (const [e, s] of this.list)
        s.update(t);
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(Os, "uuid", "fdb61dc4-2ec1-4966-b83d-54ea795fad4a");
let Ts = Os;
function oc(c, i, t, e) {
  return new Promise((s, n) => {
    function r() {
      const o = c.clientWaitSync(i, t, 0);
      if (o === c.WAIT_FAILED) {
        n();
        return;
      }
      if (o === c.TIMEOUT_EXPIRED) {
        setTimeout(r, e);
        return;
      }
      s();
    }
    r();
  });
}
async function ac(c, i, t, e, s, n, r) {
  const o = c.fenceSync(c.SYNC_GPU_COMMANDS_COMPLETE, 0);
  c.flush(), await oc(c, o, 0, 10), c.deleteSync(o), c.bindBuffer(i, t), c.getBufferSubData(i, e, s, n, r), c.bindBuffer(i, null);
}
async function no(c, i, t, e, s, n, r, o) {
  const a = c.createBuffer();
  return c.bindBuffer(c.PIXEL_PACK_BUFFER, a), c.bufferData(c.PIXEL_PACK_BUFFER, o.byteLength, c.STREAM_READ), c.readPixels(i, t, e, s, n, r, 0), c.bindBuffer(c.PIXEL_PACK_BUFFER, null), await ac(c, c.PIXEL_PACK_BUFFER, a, 0, o), c.deleteBuffer(a), o;
}
class cc extends Ge {
  constructor() {
    super(...arguments);
    C(this, "_config", {
      enabled: {
        value: !0,
        type: "Boolean"
      },
      width: {
        type: "Number",
        interpolable: !0,
        value: 512,
        min: 32,
        max: 1024
      },
      height: {
        type: "Number",
        interpolable: !0,
        value: 512,
        min: 32,
        max: 1024
      },
      autoUpdate: {
        value: !0,
        type: "Boolean"
      },
      renderDebugFrame: {
        value: !1,
        type: "Boolean"
      },
      updateInterval: {
        type: "Number",
        interpolable: !0,
        value: 1,
        min: 0,
        max: 1
      },
      threshold: {
        type: "Number",
        interpolable: !0,
        value: 100,
        min: 1,
        max: 512
      }
    });
    C(this, "_interval", null);
  }
  get enabled() {
    return this._config.enabled.value;
  }
  set enabled(t) {
    this._config.enabled.value = t, this._component.enabled = t;
  }
  get width() {
    return this._config.width.value;
  }
  set width(t) {
    this.setWidthHeight(t, this.height);
  }
  get height() {
    return this._config.height.value;
  }
  set height(t) {
    this.setWidthHeight(this.width, t);
  }
  get autoUpdate() {
    return this._config.autoUpdate.value;
  }
  set autoUpdate(t) {
    this.setAutoAndInterval(t, this.updateInterval);
  }
  get updateInterval() {
    return this._config.updateInterval.value;
  }
  set updateInterval(t) {
    this.setAutoAndInterval(this.autoUpdate, t);
  }
  get renderDebugFrame() {
    return this._config.renderDebugFrame.value;
  }
  set renderDebugFrame(t) {
    this._config.renderDebugFrame.value = t;
  }
  get threshold() {
    return this._config.threshold.value;
  }
  set threshold(t) {
    this._config.threshold.value = t;
  }
  setWidthHeight(t, e) {
    if (t <= 0 || e <= 0)
      throw new Error(
        "The width and height of the culler renderer must be more than 0!"
      );
    this._config.width.value = t, this._config.height.value = e, this.resetRenderTarget();
  }
  setAutoAndInterval(t, e) {
    if (e <= 0)
      throw new Error(
        "The updateInterval of the culler renderer must be more than 0!"
      );
    this._config.autoUpdate.value = t, this._config.updateInterval.value = e, this.resetInterval(t);
  }
  resetRenderTarget() {
    this._component.renderTarget.dispose(), this._component.renderTarget = new D.WebGLRenderTarget(
      this.width,
      this.height
    ), this._component.bufferSize = this.width * this.height * 4, this._component.buffer = new Uint8Array(this._component.bufferSize);
  }
  resetInterval(t) {
    this._interval !== null && window.clearInterval(this._interval), t && (this._interval = window.setInterval(async () => {
      this._component.preventUpdate || await this._component.updateVisibility();
    }, this.updateInterval));
  }
}
class lc {
  constructor(i, t) {
    /** {@link Configurable.onSetup} */
    C(this, "onSetup", new j());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * Fires after making the visibility check to the meshes. It lists the
     * meshes that are currently visible, and the ones that were visible
     * just before but not anymore.
     */
    C(this, "onViewUpdated", new _e());
    /**
     * Whether this renderer is active or not. If not, it won't render anything.
     */
    C(this, "enabled", !0);
    /**
     * Needs to check whether there are objects that need to be hidden or shown.
     * You can bind this to the camera movement, to a certain interval, etc.
     */
    C(this, "needsUpdate", !1);
    /** The components instance to which this renderer belongs. */
    C(this, "components");
    /** The render target used to render the visibility scene. */
    C(this, "renderTarget", new D.WebGLRenderTarget());
    /**
     * The size of the buffer where the result of the visibility check is stored.
     */
    C(this, "bufferSize", 1);
    /**
     * The buffer when the result of the visibility check is stored.
     */
    C(this, "buffer", new Uint8Array());
    /**
     * Flag to indicate if the renderer shouldn't update the visibility.
     */
    C(this, "preventUpdate", !1);
    /** {@link Configurable.config} */
    C(this, "config");
    /** {@link Configurable.isSetup} */
    C(this, "isSetup", !1);
    /** The world instance to which this renderer belongs. */
    C(this, "world");
    /** The THREE.js renderer used to make the visibility test. */
    C(this, "renderer");
    C(this, "_defaultConfig", {
      enabled: !0,
      height: 512,
      width: 512,
      updateInterval: 1e3,
      autoUpdate: !0,
      renderDebugFrame: !1,
      threshold: 100
    });
    C(this, "worker");
    C(this, "scene", new D.Scene());
    C(this, "_availableColor", 1);
    // Prevents worker being fired multiple times
    C(this, "_isWorkerBusy", !1);
    /**
     * The function that the culler uses to reprocess the scene. Generally it's
     * better to call needsUpdate, but you can also call this to force it.
     * @param force if true, it will refresh the scene even if needsUpdate is
     * not true.
     */
    C(this, "updateVisibility", async (i) => {
      if (!this.enabled || !this.needsUpdate && !i || this._isWorkerBusy)
        return;
      this._isWorkerBusy = !0;
      const t = this.world.camera.three;
      t.updateMatrix();
      const { width: e, height: s } = this.config;
      this.renderer.setSize(e, s), this.renderer.setRenderTarget(this.renderTarget), this.renderer.render(this.scene, t);
      const n = this.renderer.getContext();
      await no(
        n,
        0,
        0,
        e,
        s,
        n.RGBA,
        n.UNSIGNED_BYTE,
        this.buffer
      ), this.renderer.setRenderTarget(null), this.config.renderDebugFrame && this.renderer.render(this.scene, t), this.worker.postMessage({
        buffer: this.buffer
      }), this.needsUpdate = !1;
    });
    if (!t.renderer)
      throw new Error("The given world must have a renderer!");
    this.components = i, this.config = new cc(
      this,
      this.components,
      "Culler renderer"
    ), this.world = t, this.renderer = new D.WebGLRenderer(), this.renderer.clippingPlanes = t.renderer.clippingPlanes;
    const e = `
      addEventListener("message", (event) => {
        const { buffer } = event.data;
        const colors = new Map();
        for (let i = 0; i < buffer.length; i += 4) {
          const r = buffer[i];
          const g = buffer[i + 1];
          const b = buffer[i + 2];
          const code = "" + r + "-" + g + "-" + b;
          if(colors.has(code)) {
            colors.set(code, colors.get(code) + 1);
          } else {
            colors.set(code, 1);
          }
        }
        postMessage({ colors });
      });
    `, s = new Blob([e], { type: "application/javascript" });
    this.worker = new Worker(URL.createObjectURL(s)), this.setup();
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.enabled = !1, this.config.autoUpdate = !1, this.components.get(Ye).list.delete(this.config.uuid);
    for (const t of this.scene.children)
      t.removeFromParent();
    this.onViewUpdated.reset(), this.worker.terminate(), this.renderer.forceContextLoss(), this.renderer.dispose(), this.renderTarget.dispose(), this.buffer = null, this.onDisposed.reset();
  }
  setup(i) {
    const t = { ...this._defaultConfig, ...i }, { width: e, height: s } = t;
    this.config.setWidthHeight(e, s);
    const { updateInterval: n, autoUpdate: r } = t;
    this.config.setAutoAndInterval(r, n), this.config.threshold = t.threshold, this.isSetup = !0, this.onSetup.trigger();
  }
  getAvailableColor() {
    let i = BigInt(this._availableColor.toString());
    const t = [];
    do
      t.unshift(Number(i % 256n)), i /= 256n;
    while (i);
    for (; t.length !== 3; )
      t.unshift(0);
    const [e, s, n] = t, r = `${e}-${s}-${n}`;
    return { r: e, g: s, b: n, code: r };
  }
  increaseColor() {
    if (this._availableColor === 256 * 256 * 256) {
      console.warn("Color can't be increased over 256 x 256 x 256!");
      return;
    }
    this._availableColor++;
  }
  decreaseColor() {
    if (this._availableColor === 1) {
      console.warn("Color can't be decreased under 0!");
      return;
    }
    this._availableColor--;
  }
}
class hc extends lc {
  constructor(t, e) {
    super(t, e);
    /**
     * Event triggered when the visibility of meshes is updated.
     * Contains two sets: seen and unseen.
     */
    C(this, "onViewUpdated", new j());
    /**
     * Map of color code to THREE.InstancedMesh.
     * Used to keep track of color-coded meshes.
     */
    C(this, "colorMeshes", /* @__PURE__ */ new Map());
    C(this, "_colorCodeMeshMap", /* @__PURE__ */ new Map());
    C(this, "_meshIDColorCodeMap", /* @__PURE__ */ new Map());
    C(this, "_currentVisibleMeshes", /* @__PURE__ */ new Set());
    C(this, "_recentlyHiddenMeshes", /* @__PURE__ */ new Set());
    C(this, "_transparentMat", new D.MeshBasicMaterial({
      transparent: !0,
      opacity: 0
    }));
    C(this, "handleWorkerMessage", async (t) => {
      if (this.preventUpdate)
        return;
      const e = t.data.colors;
      this._recentlyHiddenMeshes = new Set(this._currentVisibleMeshes), this._currentVisibleMeshes.clear();
      for (const [s, n] of e) {
        if (n < this.config.threshold)
          continue;
        const r = this._colorCodeMeshMap.get(s);
        r && (this._currentVisibleMeshes.add(r), this._recentlyHiddenMeshes.delete(r));
      }
      this.onViewUpdated.trigger({
        seen: this._currentVisibleMeshes,
        unseen: this._recentlyHiddenMeshes
      }), this._isWorkerBusy = !1;
    });
    this.worker.addEventListener("message", this.handleWorkerMessage), this.onViewUpdated.add(({ seen: s, unseen: n }) => {
      for (const r of s)
        r.visible = !0;
      for (const r of n)
        r.visible = !1;
    });
  }
  /**
   * @deprecated use config.threshold instead.
   */
  get threshold() {
    return this.config.threshold;
  }
  /**
   * @deprecated use config.threshold instead.
   */
  set threshold(t) {
    this.config.threshold = t;
  }
  /** {@link Disposable.dispose} */
  dispose() {
    super.dispose(), this._currentVisibleMeshes.clear(), this._recentlyHiddenMeshes.clear(), this._meshIDColorCodeMap.clear(), this._transparentMat.dispose(), this._colorCodeMeshMap.clear();
    const t = this.components.get(De);
    for (const e in this.colorMeshes) {
      const s = this.colorMeshes.get(e);
      s && t.destroy(s, !0);
    }
    this.colorMeshes.clear();
  }
  /**
   * Adds a mesh to the culler. When the mesh is not visibile anymore, it will be removed from the scene. When it's visible again, it will be added to the scene.
   * @param mesh - The mesh to add. It can be a regular THREE.Mesh or an instance of THREE.InstancedMesh.
   */
  add(t) {
    if (!this.enabled)
      return;
    if (this.preventUpdate) {
      console.log("Culler processing not finished yet.");
      return;
    }
    this.preventUpdate = !0;
    const e = t instanceof D.InstancedMesh, { geometry: s, material: n } = t, { colorMaterial: r, code: o } = this.getAvailableMaterial();
    let a;
    if (Array.isArray(n)) {
      let f = !0;
      const I = [];
      for (const u of n)
        Tr.isTransparent(u) ? I.push(this._transparentMat) : (f = !1, I.push(r));
      if (f) {
        r.dispose(), this.preventUpdate = !1;
        return;
      }
      a = I;
    } else if (Tr.isTransparent(n)) {
      r.dispose(), this.preventUpdate = !1;
      return;
    } else
      a = r;
    this._colorCodeMeshMap.set(o, t), this._meshIDColorCodeMap.set(t.uuid, o);
    const l = e ? t.count : 1, h = new D.InstancedMesh(s, a, l);
    e ? h.instanceMatrix = t.instanceMatrix : h.setMatrixAt(0, new D.Matrix4()), t.visible = !1, t.updateWorldMatrix(!0, !1), h.applyMatrix4(t.matrixWorld), h.updateMatrix(), this.scene.add(h), this.colorMeshes.set(t.uuid, h), this.increaseColor(), this.preventUpdate = !1;
  }
  /**
   * Removes a mesh from the culler, so its visibility is not controlled by the culler anymore.
   * When the mesh is removed, it will be hidden from the scene and its color-coded mesh will be destroyed.
   * @param mesh - The mesh to remove. It can be a regular THREE.Mesh or an instance of THREE.InstancedMesh.
   */
  remove(t) {
    if (this.preventUpdate) {
      console.log("Culler processing not finished yet.");
      return;
    }
    this.preventUpdate = !0;
    const e = this.components.get(De);
    this._currentVisibleMeshes.delete(t), this._recentlyHiddenMeshes.delete(t);
    const s = this.colorMeshes.get(t.uuid), n = this._meshIDColorCodeMap.get(t.uuid);
    if (!s || !n) {
      this.preventUpdate = !1;
      return;
    }
    this._colorCodeMeshMap.delete(n), this._meshIDColorCodeMap.delete(t.uuid), this.colorMeshes.delete(t.uuid), s.geometry = void 0, s.material = [], e.destroy(s, !0), this._recentlyHiddenMeshes.delete(t), this._currentVisibleMeshes.delete(t), this.preventUpdate = !1;
  }
  /**
   * Updates the given instanced meshes inside the culler. You should use this if you change the count property, e.g. when changing the visibility of fragments.
   *
   * @param meshes - The meshes to update.
   *
   * @returns {void}
   */
  updateInstanced(t) {
    for (const e of t) {
      const s = this.colorMeshes.get(e.uuid);
      s && (s.count = e.count);
    }
  }
  getAvailableMaterial() {
    const { r: t, g: e, b: s, code: n } = this.getAvailableColor(), r = D.ColorManagement.enabled;
    D.ColorManagement.enabled = !1;
    const o = new D.Color(`rgb(${t}, ${e}, ${s})`);
    if (!this.world.renderer)
      throw new Error("Renderer not found in the world!");
    const a = this.world.renderer.clippingPlanes, l = new D.MeshBasicMaterial({
      color: o,
      clippingPlanes: a,
      side: D.DoubleSide
    });
    return D.ColorManagement.enabled = r, { colorMaterial: l, code: n };
  }
}
const Di = class Di extends At {
  constructor(t) {
    super(t);
    /**
     * An event that is triggered when the Cullers component is disposed.
     */
    C(this, "onDisposed", new j());
    C(this, "_enabled", !0);
    /**
     * A map of MeshCullerRenderer instances, keyed by their world UUIDs.
     */
    C(this, "list", /* @__PURE__ */ new Map());
    t.add(Di.uuid, this);
  }
  /** {@link Component.enabled} */
  get enabled() {
    return this._enabled;
  }
  /** {@link Component.enabled} */
  set enabled(t) {
    this._enabled = t;
    for (const [e, s] of this.list)
      s.enabled = t;
  }
  /**
   * Creates a new MeshCullerRenderer for the given world.
   * If a MeshCullerRenderer already exists for the world, it will return the existing one.
   *
   * @param world - The world for which to create the MeshCullerRenderer.
   *
   * @returns The newly created or existing MeshCullerRenderer for the given world.
   */
  create(t) {
    if (this.list.has(t.uuid))
      return this.list.get(t.uuid);
    const e = new hc(this.components, t);
    return this.list.set(t.uuid, e), e;
  }
  /**
   * Deletes the MeshCullerRenderer associated with the given world.
   * If a MeshCullerRenderer exists for the given world, it will be disposed and removed from the list.
   *
   * @param world - The world for which to delete the MeshCullerRenderer.
   *
   * @returns {void}
   */
  delete(t) {
    const e = this.list.get(t.uuid);
    e && e.dispose(), this.list.delete(t.uuid);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.enabled = !1, this.onDisposed.trigger(Di.uuid), this.onDisposed.reset();
    for (const [t, e] of this.list)
      e.dispose();
    this.list.clear();
  }
  /**
   * Updates the given instanced meshes inside the all the cullers. You should use this if you change the count property, e.g. when changing the visibility of fragments.
   *
   * @param meshes - The meshes to update.
   *
   */
  updateInstanced(t) {
    for (const [, e] of this.list)
      e.updateInstanced(t);
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(Di, "uuid", "69f2a50d-c266-44fc-b1bd-fa4d34be89e6");
let Sn = Di;
class uc {
  constructor(i, t) {
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * Fires after making the visibility check to the meshes. It lists the
     * meshes that are currently visible, and the ones that were visible
     * just before but not anymore.
     */
    C(this, "onDistanceComputed", new j());
    /**
     * Objects that won't be taken into account in the distance check.
     */
    C(this, "excludedObjects", /* @__PURE__ */ new Set());
    /**
     * Whether this renderer is active or not. If not, it won't render anything.
     */
    C(this, "enabled", !0);
    /**
     * Render the internal scene used to determine the object visibility. Used
     * for debugging purposes.
     */
    C(this, "renderDebugFrame", !1);
    /** The components instance to which this renderer belongs. */
    C(this, "components");
    /**
     * The scene where the distance is computed.
     */
    C(this, "scene", new D.Scene());
    /**
     * The camera used to compute the distance.
     */
    C(this, "camera", new D.OrthographicCamera(-1, 1, 1, -1, 0, 1));
    /**
     * The material used to compute the distance.
     */
    C(this, "depthMaterial");
    /** The world instance to which this renderer belongs. */
    C(this, "world");
    /** The THREE.js renderer used to make the visibility test. */
    C(this, "renderer");
    C(this, "worker");
    C(this, "_width", 512);
    C(this, "_height", 512);
    C(this, "_postQuad");
    C(this, "tempRT");
    C(this, "resultRT");
    C(this, "bufferSize");
    C(this, "_buffer");
    // Prevents worker being fired multiple times
    C(this, "_isWorkerBusy", !1);
    /**
     * The function that the culler uses to reprocess the scene. Generally it's
     * better to call needsUpdate, but you can also call this to force it.
     * @param force if true, it will refresh the scene even if needsUpdate is
     * not true.
     */
    C(this, "compute", async () => {
      if (!this.enabled || this.world.isDisposing || this._isWorkerBusy)
        return;
      this._isWorkerBusy = !0, this.world.camera.three.updateMatrix(), this.renderer.setSize(this._width, this._height), this.renderer.setRenderTarget(this.tempRT);
      const i = "visibilityBeforeDistanceCheck";
      for (const e of this.excludedObjects)
        e.userData[i] = e.visible, e.visible = !1;
      this.renderer.render(this.world.scene.three, this.world.camera.three);
      for (const e of this.excludedObjects)
        e.userData[i] !== void 0 && (e.visible = e.userData[i]);
      this.depthMaterial.uniforms.tDiffuse.value = this.tempRT.texture, this.depthMaterial.uniforms.tDepth.value = this.tempRT.depthTexture, this.renderer.setRenderTarget(this.resultRT), this.renderer.render(this.scene, this.camera);
      const t = this.renderer.getContext();
      try {
        await no(
          t,
          0,
          0,
          this._width,
          this._height,
          t.RGBA,
          t.UNSIGNED_BYTE,
          this._buffer
        );
      } catch {
        this.renderer.setRenderTarget(null), this._isWorkerBusy = !1;
        return;
      }
      this.renderer.setRenderTarget(null), this.renderDebugFrame && this.renderer.render(this.scene, this.camera), this.worker.postMessage({
        buffer: this._buffer
      });
    });
    C(this, "handleWorkerMessage", (i) => {
      if (!this.enabled || this.world.isDisposing)
        return;
      const t = i.data.colors;
      let e = Number.MAX_VALUE;
      for (const a of t)
        a !== 0 && a < e && (e = a);
      const s = this.world.camera.three || D.OrthographicCamera, r = (e / 255 - 1) * -1 * (s.far - s.near), o = Math.min(r, s.far);
      this.onDistanceComputed.trigger(o), this._isWorkerBusy = !1;
    });
    if (!t.renderer)
      throw new Error("The given world must have a renderer!");
    this.components = i, this.world = t;
    const e = t.camera.three;
    this.renderer = new D.WebGLRenderer(), this.tempRT = new D.WebGLRenderTarget(this._width, this._height), this.bufferSize = this._width * this._height * 4, this._buffer = new Uint8Array(this.bufferSize), this.tempRT.texture.minFilter = D.NearestFilter, this.tempRT.texture.magFilter = D.NearestFilter, this.tempRT.stencilBuffer = !1, this.tempRT.samples = 0, this.tempRT.depthTexture = new D.DepthTexture(
      this._width,
      this._height
    ), this.tempRT.depthTexture.format = D.DepthFormat, this.tempRT.depthTexture.type = D.UnsignedShortType, this.resultRT = new D.WebGLRenderTarget(this._width, this._height), this.depthMaterial = new D.ShaderMaterial({
      vertexShader: `
varying vec2 vUv;

void main() {
  vUv = uv;
  gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
}
    `,
      fragmentShader: `
#include <packing>

varying vec2 vUv;
uniform sampler2D tDiffuse;
uniform sampler2D tDepth;
uniform float cameraNear;
uniform float cameraFar;


float readDepth( sampler2D depthSampler, vec2 coord ) {
  float fragCoordZ = texture2D( depthSampler, coord ).x;
  float viewZ = perspectiveDepthToViewZ( fragCoordZ, cameraNear, cameraFar );
  return viewZToOrthographicDepth( viewZ, cameraNear, cameraFar );
}

void main() {
  //vec3 diffuse = texture2D( tDiffuse, vUv ).rgb;
  float depth = readDepth( tDepth, vUv );

  gl_FragColor.rgb = 1.0 - vec3( depth );
  gl_FragColor.a = 1.0;
}
    `,
      uniforms: {
        cameraNear: { value: e.near },
        cameraFar: { value: e.far },
        tDiffuse: { value: null },
        tDepth: { value: null }
      }
    });
    const s = new D.PlaneGeometry(2, 2);
    this._postQuad = new D.Mesh(s, this.depthMaterial), this.scene.add(this._postQuad), this.renderer.clippingPlanes = t.renderer.clippingPlanes;
    const n = `
      addEventListener("message", (event) => {
        const { buffer } = event.data;
        const colors = new Set();
        for (let i = 0; i < buffer.length; i += 4) {
          const r = buffer[i];
          colors.add(r);
        }
        postMessage({ colors });
      });
    `, r = new Blob([n], { type: "application/javascript" });
    this.worker = new Worker(URL.createObjectURL(r)), this.worker.addEventListener("message", this.handleWorkerMessage);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.enabled = !1, this.onDistanceComputed.reset(), this.worker.terminate(), this.renderer.forceContextLoss(), this.renderer.dispose(), this.tempRT.dispose(), this.resultRT.dispose();
    const i = [...this.scene.children];
    this.excludedObjects.clear();
    for (const t of i)
      t.removeFromParent();
    this._postQuad.geometry.dispose(), this._postQuad.removeFromParent(), this._buffer = null, this.onDisposed.reset();
  }
}
class Rh extends ec {
  constructor() {
    super(...arguments);
    C(this, "_distanceRenderer");
    /**
     * Whether the bias property should be set automatically depending on the shadow distance.
     */
    C(this, "autoBias", !0);
    C(this, "_defaultShadowConfig", {
      cascade: 1,
      resolution: 512
    });
    C(this, "_lightsWithShadow", /* @__PURE__ */ new Map());
    C(this, "_isComputingShadows", !1);
    C(this, "_shadowsEnabled", !0);
    C(this, "_bias", 0);
    C(this, "recomputeShadows", (t) => {
      if (!this._shadowsEnabled)
        return;
      if (this.autoBias && (this.bias = t / -1e5), t *= 1.5, !this.currentWorld)
        throw new Error(
          "A world needs to be assigned to the scene before computing shadows!"
        );
      if (!this._lightsWithShadow.size)
        throw new Error("No shadows found!");
      const s = this.currentWorld.camera.three;
      if (!(s instanceof D.PerspectiveCamera) && !(s instanceof D.OrthographicCamera))
        throw new Error("Invalid camera type!");
      const n = new D.Vector3();
      s.getWorldDirection(n);
      let r = t;
      const o = new D.Vector3();
      o.copy(this.config.directionalLight.position), o.normalize();
      for (const [a, l] of this._lightsWithShadow) {
        const h = this.directionalLights.get(l);
        if (!h)
          throw new Error("Light not found.");
        const f = new D.Vector3();
        f.copy(n);
        const I = a === this._lightsWithShadow.size - 1, u = I ? r / 2 : r * 2 / 3;
        f.multiplyScalar(u), f.add(s.position);
        const d = r - u, E = new D.Vector3();
        E.copy(o), E.multiplyScalar(d), h.target.position.copy(f), h.position.copy(f), h.position.add(E), h.shadow.camera.right = d, h.shadow.camera.left = -d, h.shadow.camera.top = d, h.shadow.camera.bottom = -d, h.shadow.camera.far = d * 2, h.shadow.camera.updateProjectionMatrix(), h.shadow.camera.updateMatrix(), I || (r /= 3);
      }
      this._isComputingShadows = !1;
    });
  }
  /**
   * The getter for the bias to prevent artifacts (stripes). It usually ranges between 0 and -0.005.
   */
  get bias() {
    return this._bias;
  }
  /**
   * The setter for the bias to prevent artifacts (stripes). It usually ranges between 0 and -0.005.
   */
  set bias(t) {
    this._bias = t;
    for (const [, e] of this._lightsWithShadow) {
      const s = this.directionalLights.get(e);
      s && (s.shadow.bias = t);
    }
  }
  /**
   * Getter to see whether the shadows are enabled or not in this scene instance.
   */
  get shadowsEnabled() {
    return this._shadowsEnabled;
  }
  /**
   * Setter to control whether the shadows are enabled or not in this scene instance.
   */
  set shadowsEnabled(t) {
    this._shadowsEnabled = t;
    for (const [, e] of this.directionalLights)
      e.castShadow = t;
  }
  /**
   * Getter to get the renderer used to determine the farthest distance from the camera.
   */
  get distanceRenderer() {
    if (!this._distanceRenderer)
      throw new Error(
        "You must set up this component before accessing the distance renderer!"
      );
    return this._distanceRenderer;
  }
  /** {@link Configurable.setup} */
  setup(t) {
    super.setup(t);
    const e = {
      ...this._defaultConfig,
      ...this._defaultShadowConfig,
      ...t
    };
    if (e.cascade <= 0)
      throw new Error(
        "Config.shadows.cascade must be a natural number greater than 0!"
      );
    if (e.cascade > 1)
      throw new Error("Multiple shadows not supported yet!");
    if (!this.currentWorld)
      throw new Error(
        "A world needs to be assigned to the scene before setting it up!"
      );
    for (const [, s] of this.directionalLights)
      s.target.removeFromParent(), s.removeFromParent(), s.dispose();
    this.directionalLights.clear(), this._distanceRenderer || (this._distanceRenderer = new uc(
      this.components,
      this.currentWorld
    ), this._distanceRenderer.onDistanceComputed.add(this.recomputeShadows)), this._lightsWithShadow.clear();
    for (let s = 0; s < e.cascade; s++) {
      const n = new D.DirectionalLight();
      n.intensity = this.config.directionalLight.intensity, n.color = this.config.directionalLight.color, n.position.copy(this.config.directionalLight.position), n.shadow.mapSize.width = e.resolution, n.shadow.mapSize.height = e.resolution, this.three.add(n, n.target), this.directionalLights.set(n.uuid, n), this._lightsWithShadow.set(s, n.uuid), n.castShadow = !0, n.shadow.bias = this._bias;
    }
  }
  /** {@link Disposable.dispose} */
  dispose() {
    super.dispose(), this._distanceRenderer && this._distanceRenderer.dispose(), this._lightsWithShadow.clear();
  }
  /** Update all the shadows of the scene. */
  async updateShadows() {
    this._isComputingShadows || !this._shadowsEnabled || (this._isComputingShadows = !0, await this.distanceRenderer.compute());
  }
}
class fc {
  constructor(i) {
    C(this, "_event");
    C(this, "_position", new D.Vector2());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    C(this, "updateMouseInfo", (i) => {
      this._event = i;
    });
    this.dom = i, this.setupEvents(!0);
  }
  /**
   * The real position of the mouse of the Three.js canvas.
   */
  get position() {
    if (this._event) {
      const i = this.dom.getBoundingClientRect();
      this._position.x = this.getPositionX(i, this._event), this._position.y = this.getPositionY(i, this._event);
    }
    return this._position;
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.setupEvents(!1), this.onDisposed.trigger(), this.onDisposed.reset();
  }
  getPositionY(i, t) {
    return -((this.getDataObject(t).clientY - i.top) / (i.bottom - i.top)) * 2 + 1;
  }
  getPositionX(i, t) {
    return (this.getDataObject(t).clientX - i.left) / (i.right - i.left) * 2 - 1;
  }
  getDataObject(i) {
    return i instanceof MouseEvent ? i : i.touches[0];
  }
  setupEvents(i) {
    i ? (this.dom.addEventListener("pointermove", this.updateMouseInfo), this.dom.addEventListener("touchstart", this.updateMouseInfo)) : (this.dom.removeEventListener("pointermove", this.updateMouseInfo), this.dom.removeEventListener("touchstart", this.updateMouseInfo));
  }
}
class Ic {
  constructor(i, t) {
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    /** The components instance to which this Raycaster belongs. */
    C(this, "components");
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /** The position of the mouse in the screen. */
    C(this, "mouse");
    /**
     * A reference to the Three.js Raycaster instance.
     * This is used for raycasting operations.
     */
    C(this, "three", new D.Raycaster());
    /**
     * A reference to the world instance to which this Raycaster belongs.
     * This is used to access the camera and meshes.
     */
    C(this, "world");
    const e = t.renderer;
    if (!e)
      throw new Error("A renderer is needed for the raycaster to work!");
    this.world = t, this.mouse = new fc(e.three.domElement), this.components = i;
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.mouse.dispose(), this.onDisposed.trigger(), this.onDisposed.reset();
  }
  /**
   * Throws a ray from the camera to the mouse or touch event point and returns
   * the first item found. This also takes into account the clipping planes
   * used by the renderer.
   *
   * @param items - the [meshes](https://threejs.org/docs/#api/en/objects/Mesh)
   * to query. If not provided, it will query all the meshes stored in
   * {@link Components.meshes}.
   */
  castRay(i = Array.from(this.world.meshes)) {
    if (!this.world)
      throw new Error("A world is needed to cast rays!");
    const t = this.world.camera.three;
    return this.three.setFromCamera(this.mouse.position, t), this.intersect(i);
  }
  /**
   * Casts a ray from a given origin in a given direction and returns the first item found.
   * This method also takes into account the clipping planes used by the renderer.
   *
   * @param origin - The origin of the ray.
   * @param direction - The direction of the ray.
   * @param items - The meshes to query. If not provided, it will query all the meshes stored in {@link World.meshes}.
   * @returns The first intersection found or `null` if no intersection was found.
   */
  castRayFromVector(i, t, e = Array.from(this.world.meshes)) {
    return this.three.set(i, t), this.intersect(e);
  }
  intersect(i = Array.from(this.world.meshes)) {
    const t = this.three.intersectObjects(i), e = this.filterClippingPlanes(t);
    return e.length > 0 ? e[0] : null;
  }
  filterClippingPlanes(i) {
    if (!this.world.renderer)
      throw new Error("Renderer not found!");
    const t = this.world.renderer.three;
    if (!t.clippingPlanes)
      return i;
    const e = t.clippingPlanes;
    return i.length <= 0 || !e || (e == null ? void 0 : e.length) <= 0 ? i : i.filter(
      (s) => e.every((n) => n.distanceToPoint(s.point) > 0)
    );
  }
}
const Ns = class Ns extends At {
  constructor(t) {
    super(t);
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    /**
     * A Map that stores raycasters for each world.
     * The key is the world's UUID, and the value is the corresponding SimpleRaycaster instance.
     */
    C(this, "list", /* @__PURE__ */ new Map());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    t.add(Ns.uuid, this);
  }
  /**
   * Retrieves a SimpleRaycaster instance for the given world.
   * If a SimpleRaycaster instance already exists for the world, it will be returned.
   * Otherwise, a new SimpleRaycaster instance will be created and added to the list.
   *
   * @param world - The world for which to retrieve or create a SimpleRaycaster instance.
   * @returns The SimpleRaycaster instance for the given world.
   */
  get(t) {
    if (this.list.has(t.uuid))
      return this.list.get(t.uuid);
    const e = new Ic(this.components, t);
    return this.list.set(t.uuid, e), t.onDisposed.add(() => {
      this.delete(t);
    }), e;
  }
  /**
   * Deletes the SimpleRaycaster instance associated with the given world.
   * If a SimpleRaycaster instance exists for the given world, it will be disposed and removed from the list.
   *
   * @param world - The world for which to delete the SimpleRaycaster instance.
   * @returns {void}
   */
  delete(t) {
    const e = this.list.get(t.uuid);
    e && e.dispose(), this.list.delete(t.uuid);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    for (const [t, e] of this.list)
      e.dispose();
    this.list.clear(), this.onDisposed.trigger();
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(Ns, "uuid", "d5d8bdf0-db25-4952-b951-b643af207ace");
let ci = Ns;
class dc extends Ge {
  constructor() {
    super(...arguments);
    C(this, "_config", {
      visible: {
        value: !0,
        type: "Boolean"
      },
      color: {
        value: new D.Color(),
        type: "Color"
      },
      primarySize: {
        type: "Number",
        interpolable: !0,
        value: 1,
        min: 0,
        max: 1e3
      },
      secondarySize: {
        type: "Number",
        interpolable: !0,
        value: 10,
        min: 0,
        max: 1e3
      },
      distance: {
        type: "Number",
        interpolable: !0,
        value: 500,
        min: 0,
        max: 500
      }
    });
  }
  /**
   * Whether the grid is visible or not.
   */
  get visible() {
    return this._config.visible.value;
  }
  /**
   * Whether the grid is visible or not.
   */
  set visible(t) {
    this._config.visible.value = t, this._component.visible = t;
  }
  /**
   * The color of the grid lines.
   */
  get color() {
    return this._config.color.value;
  }
  /**
   * The color of the grid lines.
   */
  set color(t) {
    this._config.color.value = t, this._component.material.uniforms.uColor.value = t, this._component.material.uniformsNeedUpdate = !0;
  }
  /**
   * The size of the primary grid lines.
   */
  get primarySize() {
    return this._config.primarySize.value;
  }
  /**
   * The size of the primary grid lines.
   */
  set primarySize(t) {
    this._config.primarySize.value = t, this._component.material.uniforms.uSize1.value = t, this._component.material.uniformsNeedUpdate = !0;
  }
  /**
   * The size of the secondary grid lines.
   */
  get secondarySize() {
    return this._config.secondarySize.value;
  }
  /**
   * The size of the secondary grid lines.
   */
  set secondarySize(t) {
    this._config.secondarySize.value = t, this._component.material.uniforms.uSize2.value = t, this._component.material.uniformsNeedUpdate = !0;
  }
  /**
   * The distance at which the grid lines start to fade away.
   */
  get distance() {
    return this._config.distance.value;
  }
  /**
   * The distance at which the grid lines start to fade away.
   */
  set distance(t) {
    this._config.distance.value = t, this._component.material.uniforms.uDistance.value = t, this._component.material.uniformsNeedUpdate = !0;
  }
}
class Ec {
  constructor(i, t) {
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /** {@link Configurable.onSetup} */
    C(this, "onSetup", new j());
    /** {@link Configurable.isSetup} */
    C(this, "isSetup", !1);
    /** The world instance to which this Raycaster belongs. */
    C(this, "world");
    /** The components instance to which this grid belongs. */
    C(this, "components");
    /** {@link Configurable.config} */
    C(this, "config");
    C(this, "_defaultConfig", {
      visible: !0,
      color: new D.Color(12303291),
      primarySize: 1,
      secondarySize: 10,
      distance: 500
    });
    /** The Three.js mesh that contains the infinite grid. */
    C(this, "three");
    C(this, "_fade", 3);
    C(this, "updateZoom", () => {
      this.world.camera instanceof xi && (this.material.uniforms.uZoom.value = this.world.camera.three.zoom);
    });
    this.world = t;
    const { color: e, primarySize: s, secondarySize: n, distance: r } = this._defaultConfig;
    this.components = i, this.config = new dc(this, this.components, "Grid");
    const o = new D.PlaneGeometry(2, 2, 1, 1), a = new D.ShaderMaterial({
      side: D.DoubleSide,
      uniforms: {
        uSize1: {
          value: s
        },
        uSize2: {
          value: n
        },
        uColor: {
          value: e
        },
        uDistance: {
          value: r
        },
        uFade: {
          value: this._fade
        },
        uZoom: {
          value: 1
        }
      },
      transparent: !0,
      vertexShader: `
            
            varying vec3 worldPosition;
            
            uniform float uDistance;
            
            void main() {
            
                    vec3 pos = position.xzy * uDistance;
                    pos.xz += cameraPosition.xz;
                    
                    worldPosition = pos;
                    
                    gl_Position = projectionMatrix * modelViewMatrix * vec4(pos, 1.0);
            
            }
            `,
      fragmentShader: `
            
            varying vec3 worldPosition;
            
            uniform float uZoom;
            uniform float uFade;
            uniform float uSize1;
            uniform float uSize2;
            uniform vec3 uColor;
            uniform float uDistance;
                
                
                
                float getGrid(float size) {
                
                    vec2 r = worldPosition.xz / size;
                    
                    
                    vec2 grid = abs(fract(r - 0.5) - 0.5) / fwidth(r);
                    float line = min(grid.x, grid.y);
                    
                
                    return 1.0 - min(line, 1.0);
                }
                
            void main() {
            
                    
                    float d = 1.0 - min(distance(cameraPosition.xz, worldPosition.xz) / uDistance, 1.0);
                    
                    float g1 = getGrid(uSize1);
                    float g2 = getGrid(uSize2);
                    
                    // Ortho camera fades the grid away when zooming out
                    float minZoom = step(0.2, uZoom);
                    float zoomFactor = pow(min(uZoom, 1.), 2.) * minZoom;
                    
                    gl_FragColor = vec4(uColor.rgb, mix(g2, g1, g1) * pow(d, uFade));
                    gl_FragColor.a = mix(0.5 * gl_FragColor.a, gl_FragColor.a, g2) * zoomFactor;
                    
                    if ( gl_FragColor.a <= 0.0 ) discard;
                    
            
            }
            
            `,
      extensions: {
        derivatives: !0
      }
    });
    this.three = new D.Mesh(o, a), this.three.frustumCulled = !1, t.scene.three.add(this.three), this.setupEvents(!0);
  }
  /** {@link Hideable.visible} */
  get visible() {
    return this.three.visible;
  }
  /** {@link Hideable.visible} */
  set visible(i) {
    i ? this.world.scene.three.add(this.three) : this.three.removeFromParent();
  }
  /** The material of the grid. */
  get material() {
    return this.three.material;
  }
  /**
   * Whether the grid should fade away with distance. Recommended to be true for
   * perspective cameras and false for orthographic cameras.
   */
  get fade() {
    return this._fade === 3;
  }
  /**
   * Whether the grid should fade away with distance. Recommended to be true for
   * perspective cameras and false for orthographic cameras.
   */
  set fade(i) {
    this._fade = i ? 3 : 0, this.material.uniforms.uFade.value = this._fade;
  }
  /** {@link Configurable.setup} */
  setup(i) {
    const t = { ...this._defaultConfig, ...i };
    this.config.visible = !0, this.config.color = t.color, this.config.primarySize = t.primarySize, this.config.secondarySize = t.secondarySize, this.config.distance = t.distance, this.isSetup = !0, this.onSetup.trigger();
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.setupEvents(!1), this.components.get(Ye).list.delete(this.config.uuid), this.components.get(De).destroy(this.three), this.onDisposed.trigger(), this.onDisposed.reset(), this.world = null, this.components = null;
  }
  setupEvents(i) {
    if (this.world.isDisposing || !(this.world.camera instanceof xi))
      return;
    const t = this.world.camera.controls;
    i ? t.addEventListener("update", this.updateZoom) : t.removeEventListener("update", this.updateZoom);
  }
}
const ys = class ys extends At {
  constructor(t) {
    super(t);
    /**
     * A map of world UUIDs to their corresponding grid instances.
     */
    C(this, "list", /* @__PURE__ */ new Map());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    t.add(ys.uuid, this);
  }
  /**
   * Creates a new grid for the given world.
   * Throws an error if a grid already exists for the world.
   *
   * @param world - The world to create the grid for.
   * @returns The newly created grid.
   *
   * @throws Will throw an error if a grid already exists for the given world.
   */
  create(t) {
    if (this.list.has(t.uuid))
      throw new Error("This world already has a grid!");
    const e = new Ec(this.components, t);
    return this.list.set(t.uuid, e), t.onDisposed.add(() => {
      this.delete(t);
    }), e;
  }
  /**
   * Deletes the grid associated with the given world.
   * If a grid does not exist for the given world, this method does nothing.
   *
   * @param world - The world for which to delete the grid.
   *
   * @remarks
   * This method will dispose of the grid and remove it from the internal list.
   * If the world is disposed before calling this method, the grid will be automatically deleted.
   */
  delete(t) {
    const e = this.list.get(t.uuid);
    e && e.dispose(), this.list.delete(t.uuid);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    for (const [t, e] of this.list)
      e.dispose();
    this.list.clear(), this.onDisposed.trigger(), this.onDisposed.reset();
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(ys, "uuid", "d1e814d5-b81c-4452-87a2-f039375e0489");
let yr = ys;
const xe = new Lo(), bt = new Q(), Oe = new Q(), Ot = new de(), Lr = {
  X: new Q(1, 0, 0),
  Y: new Q(0, 1, 0),
  Z: new Q(0, 0, 1)
}, fn = { type: "change" }, Pr = { type: "mouseDown" }, _r = { type: "mouseUp", mode: null }, wr = { type: "objectChange" };
class pc extends dn {
  constructor(i, t) {
    super(), t === void 0 && (console.warn('THREE.TransformControls: The second parameter "domElement" is now mandatory.'), t = document), this.isTransformControls = !0, this.visible = !1, this.domElement = t, this.domElement.style.touchAction = "none";
    const e = new Ac();
    this._gizmo = e, this.add(e);
    const s = new Fc();
    this._plane = s, this.add(s);
    const n = this;
    function r(R, S) {
      let m = S;
      Object.defineProperty(n, R, {
        get: function() {
          return m !== void 0 ? m : S;
        },
        set: function(F) {
          m !== F && (m = F, s[R] = F, e[R] = F, n.dispatchEvent({ type: R + "-changed", value: F }), n.dispatchEvent(fn));
        }
      }), n[R] = S, s[R] = S, e[R] = S;
    }
    r("camera", i), r("object", void 0), r("enabled", !0), r("axis", null), r("mode", "translate"), r("translationSnap", null), r("rotationSnap", null), r("scaleSnap", null), r("space", "world"), r("size", 1), r("dragging", !1), r("showX", !0), r("showY", !0), r("showZ", !0);
    const o = new Q(), a = new Q(), l = new de(), h = new de(), f = new Q(), I = new de(), u = new Q(), d = new Q(), E = new Q(), T = 0, p = new Q();
    r("worldPosition", o), r("worldPositionStart", a), r("worldQuaternion", l), r("worldQuaternionStart", h), r("cameraPosition", f), r("cameraQuaternion", I), r("pointStart", u), r("pointEnd", d), r("rotationAxis", E), r("rotationAngle", T), r("eye", p), this._offset = new Q(), this._startNorm = new Q(), this._endNorm = new Q(), this._cameraScale = new Q(), this._parentPosition = new Q(), this._parentQuaternion = new de(), this._parentQuaternionInv = new de(), this._parentScale = new Q(), this._worldScaleStart = new Q(), this._worldQuaternionInv = new de(), this._worldScale = new Q(), this._positionStart = new Q(), this._quaternionStart = new de(), this._scaleStart = new Q(), this._getPointer = Cc.bind(this), this._onPointerDown = mc.bind(this), this._onPointerHover = Tc.bind(this), this._onPointerMove = Rc.bind(this), this._onPointerUp = gc.bind(this), this.domElement.addEventListener("pointerdown", this._onPointerDown), this.domElement.addEventListener("pointermove", this._onPointerHover), this.domElement.addEventListener("pointerup", this._onPointerUp);
  }
  // updateMatrixWorld  updates key transformation variables
  updateMatrixWorld() {
    this.object !== void 0 && (this.object.updateMatrixWorld(), this.object.parent === null ? console.error("TransformControls: The attached 3D object must be a part of the scene graph.") : this.object.parent.matrixWorld.decompose(this._parentPosition, this._parentQuaternion, this._parentScale), this.object.matrixWorld.decompose(this.worldPosition, this.worldQuaternion, this._worldScale), this._parentQuaternionInv.copy(this._parentQuaternion).invert(), this._worldQuaternionInv.copy(this.worldQuaternion).invert()), this.camera.updateMatrixWorld(), this.camera.matrixWorld.decompose(this.cameraPosition, this.cameraQuaternion, this._cameraScale), this.camera.isOrthographicCamera ? this.camera.getWorldDirection(this.eye).negate() : this.eye.copy(this.cameraPosition).sub(this.worldPosition).normalize(), super.updateMatrixWorld(this);
  }
  pointerHover(i) {
    if (this.object === void 0 || this.dragging === !0)
      return;
    xe.setFromCamera(i, this.camera);
    const t = In(this._gizmo.picker[this.mode], xe);
    t ? this.axis = t.object.name : this.axis = null;
  }
  pointerDown(i) {
    if (!(this.object === void 0 || this.dragging === !0 || i.button !== 0) && this.axis !== null) {
      xe.setFromCamera(i, this.camera);
      const t = In(this._plane, xe, !0);
      t && (this.object.updateMatrixWorld(), this.object.parent.updateMatrixWorld(), this._positionStart.copy(this.object.position), this._quaternionStart.copy(this.object.quaternion), this._scaleStart.copy(this.object.scale), this.object.matrixWorld.decompose(this.worldPositionStart, this.worldQuaternionStart, this._worldScaleStart), this.pointStart.copy(t.point).sub(this.worldPositionStart)), this.dragging = !0, Pr.mode = this.mode, this.dispatchEvent(Pr);
    }
  }
  pointerMove(i) {
    const t = this.axis, e = this.mode, s = this.object;
    let n = this.space;
    if (e === "scale" ? n = "local" : (t === "E" || t === "XYZE" || t === "XYZ") && (n = "world"), s === void 0 || t === null || this.dragging === !1 || i.button !== -1)
      return;
    xe.setFromCamera(i, this.camera);
    const r = In(this._plane, xe, !0);
    if (r) {
      if (this.pointEnd.copy(r.point).sub(this.worldPositionStart), e === "translate")
        this._offset.copy(this.pointEnd).sub(this.pointStart), n === "local" && t !== "XYZ" && this._offset.applyQuaternion(this._worldQuaternionInv), t.indexOf("X") === -1 && (this._offset.x = 0), t.indexOf("Y") === -1 && (this._offset.y = 0), t.indexOf("Z") === -1 && (this._offset.z = 0), n === "local" && t !== "XYZ" ? this._offset.applyQuaternion(this._quaternionStart).divide(this._parentScale) : this._offset.applyQuaternion(this._parentQuaternionInv).divide(this._parentScale), s.position.copy(this._offset).add(this._positionStart), this.translationSnap && (n === "local" && (s.position.applyQuaternion(Ot.copy(this._quaternionStart).invert()), t.search("X") !== -1 && (s.position.x = Math.round(s.position.x / this.translationSnap) * this.translationSnap), t.search("Y") !== -1 && (s.position.y = Math.round(s.position.y / this.translationSnap) * this.translationSnap), t.search("Z") !== -1 && (s.position.z = Math.round(s.position.z / this.translationSnap) * this.translationSnap), s.position.applyQuaternion(this._quaternionStart)), n === "world" && (s.parent && s.position.add(bt.setFromMatrixPosition(s.parent.matrixWorld)), t.search("X") !== -1 && (s.position.x = Math.round(s.position.x / this.translationSnap) * this.translationSnap), t.search("Y") !== -1 && (s.position.y = Math.round(s.position.y / this.translationSnap) * this.translationSnap), t.search("Z") !== -1 && (s.position.z = Math.round(s.position.z / this.translationSnap) * this.translationSnap), s.parent && s.position.sub(bt.setFromMatrixPosition(s.parent.matrixWorld))));
      else if (e === "scale") {
        if (t.search("XYZ") !== -1) {
          let o = this.pointEnd.length() / this.pointStart.length();
          this.pointEnd.dot(this.pointStart) < 0 && (o *= -1), Oe.set(o, o, o);
        } else
          bt.copy(this.pointStart), Oe.copy(this.pointEnd), bt.applyQuaternion(this._worldQuaternionInv), Oe.applyQuaternion(this._worldQuaternionInv), Oe.divide(bt), t.search("X") === -1 && (Oe.x = 1), t.search("Y") === -1 && (Oe.y = 1), t.search("Z") === -1 && (Oe.z = 1);
        s.scale.copy(this._scaleStart).multiply(Oe), this.scaleSnap && (t.search("X") !== -1 && (s.scale.x = Math.round(s.scale.x / this.scaleSnap) * this.scaleSnap || this.scaleSnap), t.search("Y") !== -1 && (s.scale.y = Math.round(s.scale.y / this.scaleSnap) * this.scaleSnap || this.scaleSnap), t.search("Z") !== -1 && (s.scale.z = Math.round(s.scale.z / this.scaleSnap) * this.scaleSnap || this.scaleSnap));
      } else if (e === "rotate") {
        this._offset.copy(this.pointEnd).sub(this.pointStart);
        const o = 20 / this.worldPosition.distanceTo(bt.setFromMatrixPosition(this.camera.matrixWorld));
        let a = !1;
        t === "XYZE" ? (this.rotationAxis.copy(this._offset).cross(this.eye).normalize(), this.rotationAngle = this._offset.dot(bt.copy(this.rotationAxis).cross(this.eye)) * o) : (t === "X" || t === "Y" || t === "Z") && (this.rotationAxis.copy(Lr[t]), bt.copy(Lr[t]), n === "local" && bt.applyQuaternion(this.worldQuaternion), bt.cross(this.eye), bt.length() === 0 ? a = !0 : this.rotationAngle = this._offset.dot(bt.normalize()) * o), (t === "E" || a) && (this.rotationAxis.copy(this.eye), this.rotationAngle = this.pointEnd.angleTo(this.pointStart), this._startNorm.copy(this.pointStart).normalize(), this._endNorm.copy(this.pointEnd).normalize(), this.rotationAngle *= this._endNorm.cross(this._startNorm).dot(this.eye) < 0 ? 1 : -1), this.rotationSnap && (this.rotationAngle = Math.round(this.rotationAngle / this.rotationSnap) * this.rotationSnap), n === "local" && t !== "E" && t !== "XYZE" ? (s.quaternion.copy(this._quaternionStart), s.quaternion.multiply(Ot.setFromAxisAngle(this.rotationAxis, this.rotationAngle)).normalize()) : (this.rotationAxis.applyQuaternion(this._parentQuaternionInv), s.quaternion.copy(Ot.setFromAxisAngle(this.rotationAxis, this.rotationAngle)), s.quaternion.multiply(this._quaternionStart).normalize());
      }
      this.dispatchEvent(fn), this.dispatchEvent(wr);
    }
  }
  pointerUp(i) {
    i.button === 0 && (this.dragging && this.axis !== null && (_r.mode = this.mode, this.dispatchEvent(_r)), this.dragging = !1, this.axis = null);
  }
  dispose() {
    this.domElement.removeEventListener("pointerdown", this._onPointerDown), this.domElement.removeEventListener("pointermove", this._onPointerHover), this.domElement.removeEventListener("pointermove", this._onPointerMove), this.domElement.removeEventListener("pointerup", this._onPointerUp), this.traverse(function(i) {
      i.geometry && i.geometry.dispose(), i.material && i.material.dispose();
    });
  }
  // Set current object
  attach(i) {
    return this.object = i, this.visible = !0, this;
  }
  // Detach from object
  detach() {
    return this.object = void 0, this.visible = !1, this.axis = null, this;
  }
  reset() {
    this.enabled && this.dragging && (this.object.position.copy(this._positionStart), this.object.quaternion.copy(this._quaternionStart), this.object.scale.copy(this._scaleStart), this.dispatchEvent(fn), this.dispatchEvent(wr), this.pointStart.copy(this.pointEnd));
  }
  getRaycaster() {
    return xe;
  }
  // TODO: deprecate
  getMode() {
    return this.mode;
  }
  setMode(i) {
    this.mode = i;
  }
  setTranslationSnap(i) {
    this.translationSnap = i;
  }
  setRotationSnap(i) {
    this.rotationSnap = i;
  }
  setScaleSnap(i) {
    this.scaleSnap = i;
  }
  setSize(i) {
    this.size = i;
  }
  setSpace(i) {
    this.space = i;
  }
}
function Cc(c) {
  if (this.domElement.ownerDocument.pointerLockElement)
    return {
      x: 0,
      y: 0,
      button: c.button
    };
  {
    const i = this.domElement.getBoundingClientRect();
    return {
      x: (c.clientX - i.left) / i.width * 2 - 1,
      y: -(c.clientY - i.top) / i.height * 2 + 1,
      button: c.button
    };
  }
}
function Tc(c) {
  if (this.enabled)
    switch (c.pointerType) {
      case "mouse":
      case "pen":
        this.pointerHover(this._getPointer(c));
        break;
    }
}
function mc(c) {
  this.enabled && (document.pointerLockElement || this.domElement.setPointerCapture(c.pointerId), this.domElement.addEventListener("pointermove", this._onPointerMove), this.pointerHover(this._getPointer(c)), this.pointerDown(this._getPointer(c)));
}
function Rc(c) {
  this.enabled && this.pointerMove(this._getPointer(c));
}
function gc(c) {
  this.enabled && (this.domElement.releasePointerCapture(c.pointerId), this.domElement.removeEventListener("pointermove", this._onPointerMove), this.pointerUp(this._getPointer(c)));
}
function In(c, i, t) {
  const e = i.intersectObject(c, !0);
  for (let s = 0; s < e.length; s++)
    if (e[s].object.visible || t)
      return e[s];
  return !1;
}
const cs = new Po(), mt = new Q(0, 1, 0), Mr = new Q(0, 0, 0), Dr = new pe(), ls = new de(), Es = new de(), ue = new Q(), br = new pe(), Pi = new Q(1, 0, 0), Be = new Q(0, 1, 0), _i = new Q(0, 0, 1), hs = new Q(), Oi = new Q(), Ni = new Q();
class Ac extends dn {
  constructor() {
    super(), this.isTransformControlsGizmo = !0, this.type = "TransformControlsGizmo";
    const i = new Qr({
      depthTest: !1,
      depthWrite: !1,
      fog: !1,
      toneMapped: !1,
      transparent: !0
    }), t = new _o({
      depthTest: !1,
      depthWrite: !1,
      fog: !1,
      toneMapped: !1,
      transparent: !0
    }), e = i.clone();
    e.opacity = 0.15;
    const s = t.clone();
    s.opacity = 0.5;
    const n = i.clone();
    n.color.setHex(16711680);
    const r = i.clone();
    r.color.setHex(65280);
    const o = i.clone();
    o.color.setHex(255);
    const a = i.clone();
    a.color.setHex(16711680), a.opacity = 0.5;
    const l = i.clone();
    l.color.setHex(65280), l.opacity = 0.5;
    const h = i.clone();
    h.color.setHex(255), h.opacity = 0.5;
    const f = i.clone();
    f.opacity = 0.25;
    const I = i.clone();
    I.color.setHex(16776960), I.opacity = 0.25, i.clone().color.setHex(16776960);
    const d = i.clone();
    d.color.setHex(7895160);
    const E = new Vt(0, 0.04, 0.1, 12);
    E.translate(0, 0.05, 0);
    const T = new Ut(0.08, 0.08, 0.08);
    T.translate(0, 0.04, 0);
    const p = new jn();
    p.setAttribute("position", new qn([0, 0, 0, 1, 0, 0], 3));
    const R = new Vt(75e-4, 75e-4, 0.5, 3);
    R.translate(0, 0.25, 0);
    function S(v, q) {
      const G = new Ii(v, 75e-4, 3, 64, q * Math.PI * 2);
      return G.rotateY(Math.PI / 2), G.rotateX(Math.PI / 2), G;
    }
    function m() {
      const v = new jn();
      return v.setAttribute("position", new qn([0, 0, 0, 1, 1, 1], 3)), v;
    }
    const F = {
      X: [
        [new rt(E, n), [0.5, 0, 0], [0, 0, -Math.PI / 2]],
        [new rt(E, n), [-0.5, 0, 0], [0, 0, Math.PI / 2]],
        [new rt(R, n), [0, 0, 0], [0, 0, -Math.PI / 2]]
      ],
      Y: [
        [new rt(E, r), [0, 0.5, 0]],
        [new rt(E, r), [0, -0.5, 0], [Math.PI, 0, 0]],
        [new rt(R, r)]
      ],
      Z: [
        [new rt(E, o), [0, 0, 0.5], [Math.PI / 2, 0, 0]],
        [new rt(E, o), [0, 0, -0.5], [-Math.PI / 2, 0, 0]],
        [new rt(R, o), null, [Math.PI / 2, 0, 0]]
      ],
      XYZ: [
        [new rt(new Gi(0.1, 0), f.clone()), [0, 0, 0]]
      ],
      XY: [
        [new rt(new Ut(0.15, 0.15, 0.01), h.clone()), [0.15, 0.15, 0]]
      ],
      YZ: [
        [new rt(new Ut(0.15, 0.15, 0.01), a.clone()), [0, 0.15, 0.15], [0, Math.PI / 2, 0]]
      ],
      XZ: [
        [new rt(new Ut(0.15, 0.15, 0.01), l.clone()), [0.15, 0, 0.15], [-Math.PI / 2, 0, 0]]
      ]
    }, O = {
      X: [
        [new rt(new Vt(0.2, 0, 0.6, 4), e), [0.3, 0, 0], [0, 0, -Math.PI / 2]],
        [new rt(new Vt(0.2, 0, 0.6, 4), e), [-0.3, 0, 0], [0, 0, Math.PI / 2]]
      ],
      Y: [
        [new rt(new Vt(0.2, 0, 0.6, 4), e), [0, 0.3, 0]],
        [new rt(new Vt(0.2, 0, 0.6, 4), e), [0, -0.3, 0], [0, 0, Math.PI]]
      ],
      Z: [
        [new rt(new Vt(0.2, 0, 0.6, 4), e), [0, 0, 0.3], [Math.PI / 2, 0, 0]],
        [new rt(new Vt(0.2, 0, 0.6, 4), e), [0, 0, -0.3], [-Math.PI / 2, 0, 0]]
      ],
      XYZ: [
        [new rt(new Gi(0.2, 0), e)]
      ],
      XY: [
        [new rt(new Ut(0.2, 0.2, 0.01), e), [0.15, 0.15, 0]]
      ],
      YZ: [
        [new rt(new Ut(0.2, 0.2, 0.01), e), [0, 0.15, 0.15], [0, Math.PI / 2, 0]]
      ],
      XZ: [
        [new rt(new Ut(0.2, 0.2, 0.01), e), [0.15, 0, 0.15], [-Math.PI / 2, 0, 0]]
      ]
    }, y = {
      START: [
        [new rt(new Gi(0.01, 2), s), null, null, null, "helper"]
      ],
      END: [
        [new rt(new Gi(0.01, 2), s), null, null, null, "helper"]
      ],
      DELTA: [
        [new Fe(m(), s), null, null, null, "helper"]
      ],
      X: [
        [new Fe(p, s.clone()), [-1e3, 0, 0], null, [1e6, 1, 1], "helper"]
      ],
      Y: [
        [new Fe(p, s.clone()), [0, -1e3, 0], [0, 0, Math.PI / 2], [1e6, 1, 1], "helper"]
      ],
      Z: [
        [new Fe(p, s.clone()), [0, 0, -1e3], [0, -Math.PI / 2, 0], [1e6, 1, 1], "helper"]
      ]
    }, w = {
      XYZE: [
        [new rt(S(0.5, 1), d), null, [0, Math.PI / 2, 0]]
      ],
      X: [
        [new rt(S(0.5, 0.5), n)]
      ],
      Y: [
        [new rt(S(0.5, 0.5), r), null, [0, 0, -Math.PI / 2]]
      ],
      Z: [
        [new rt(S(0.5, 0.5), o), null, [0, Math.PI / 2, 0]]
      ],
      E: [
        [new rt(S(0.75, 1), I), null, [0, Math.PI / 2, 0]]
      ]
    }, L = {
      AXIS: [
        [new Fe(p, s.clone()), [-1e3, 0, 0], null, [1e6, 1, 1], "helper"]
      ]
    }, b = {
      XYZE: [
        [new rt(new wo(0.25, 10, 8), e)]
      ],
      X: [
        [new rt(new Ii(0.5, 0.1, 4, 24), e), [0, 0, 0], [0, -Math.PI / 2, -Math.PI / 2]]
      ],
      Y: [
        [new rt(new Ii(0.5, 0.1, 4, 24), e), [0, 0, 0], [Math.PI / 2, 0, 0]]
      ],
      Z: [
        [new rt(new Ii(0.5, 0.1, 4, 24), e), [0, 0, 0], [0, 0, -Math.PI / 2]]
      ],
      E: [
        [new rt(new Ii(0.75, 0.1, 2, 24), e)]
      ]
    }, Y = {
      X: [
        [new rt(T, n), [0.5, 0, 0], [0, 0, -Math.PI / 2]],
        [new rt(R, n), [0, 0, 0], [0, 0, -Math.PI / 2]],
        [new rt(T, n), [-0.5, 0, 0], [0, 0, Math.PI / 2]]
      ],
      Y: [
        [new rt(T, r), [0, 0.5, 0]],
        [new rt(R, r)],
        [new rt(T, r), [0, -0.5, 0], [0, 0, Math.PI]]
      ],
      Z: [
        [new rt(T, o), [0, 0, 0.5], [Math.PI / 2, 0, 0]],
        [new rt(R, o), [0, 0, 0], [Math.PI / 2, 0, 0]],
        [new rt(T, o), [0, 0, -0.5], [-Math.PI / 2, 0, 0]]
      ],
      XY: [
        [new rt(new Ut(0.15, 0.15, 0.01), h), [0.15, 0.15, 0]]
      ],
      YZ: [
        [new rt(new Ut(0.15, 0.15, 0.01), a), [0, 0.15, 0.15], [0, Math.PI / 2, 0]]
      ],
      XZ: [
        [new rt(new Ut(0.15, 0.15, 0.01), l), [0.15, 0, 0.15], [-Math.PI / 2, 0, 0]]
      ],
      XYZ: [
        [new rt(new Ut(0.1, 0.1, 0.1), f.clone())]
      ]
    }, N = {
      X: [
        [new rt(new Vt(0.2, 0, 0.6, 4), e), [0.3, 0, 0], [0, 0, -Math.PI / 2]],
        [new rt(new Vt(0.2, 0, 0.6, 4), e), [-0.3, 0, 0], [0, 0, Math.PI / 2]]
      ],
      Y: [
        [new rt(new Vt(0.2, 0, 0.6, 4), e), [0, 0.3, 0]],
        [new rt(new Vt(0.2, 0, 0.6, 4), e), [0, -0.3, 0], [0, 0, Math.PI]]
      ],
      Z: [
        [new rt(new Vt(0.2, 0, 0.6, 4), e), [0, 0, 0.3], [Math.PI / 2, 0, 0]],
        [new rt(new Vt(0.2, 0, 0.6, 4), e), [0, 0, -0.3], [-Math.PI / 2, 0, 0]]
      ],
      XY: [
        [new rt(new Ut(0.2, 0.2, 0.01), e), [0.15, 0.15, 0]]
      ],
      YZ: [
        [new rt(new Ut(0.2, 0.2, 0.01), e), [0, 0.15, 0.15], [0, Math.PI / 2, 0]]
      ],
      XZ: [
        [new rt(new Ut(0.2, 0.2, 0.01), e), [0.15, 0, 0.15], [-Math.PI / 2, 0, 0]]
      ],
      XYZ: [
        [new rt(new Ut(0.2, 0.2, 0.2), e), [0, 0, 0]]
      ]
    }, M = {
      X: [
        [new Fe(p, s.clone()), [-1e3, 0, 0], null, [1e6, 1, 1], "helper"]
      ],
      Y: [
        [new Fe(p, s.clone()), [0, -1e3, 0], [0, 0, Math.PI / 2], [1e6, 1, 1], "helper"]
      ],
      Z: [
        [new Fe(p, s.clone()), [0, 0, -1e3], [0, -Math.PI / 2, 0], [1e6, 1, 1], "helper"]
      ]
    };
    function g(v) {
      const q = new dn();
      for (const G in v)
        for (let et = v[G].length; et--; ) {
          const W = v[G][et][0].clone(), nt = v[G][et][1], V = v[G][et][2], x = v[G][et][3], ot = v[G][et][4];
          W.name = G, W.tag = ot, nt && W.position.set(nt[0], nt[1], nt[2]), V && W.rotation.set(V[0], V[1], V[2]), x && W.scale.set(x[0], x[1], x[2]), W.updateMatrix();
          const it = W.geometry.clone();
          it.applyMatrix4(W.matrix), W.geometry = it, W.renderOrder = 1 / 0, W.position.set(0, 0, 0), W.rotation.set(0, 0, 0), W.scale.set(1, 1, 1), q.add(W);
        }
      return q;
    }
    this.gizmo = {}, this.picker = {}, this.helper = {}, this.add(this.gizmo.translate = g(F)), this.add(this.gizmo.rotate = g(w)), this.add(this.gizmo.scale = g(Y)), this.add(this.picker.translate = g(O)), this.add(this.picker.rotate = g(b)), this.add(this.picker.scale = g(N)), this.add(this.helper.translate = g(y)), this.add(this.helper.rotate = g(L)), this.add(this.helper.scale = g(M)), this.picker.translate.visible = !1, this.picker.rotate.visible = !1, this.picker.scale.visible = !1;
  }
  // updateMatrixWorld will update transformations and appearance of individual handles
  updateMatrixWorld(i) {
    const e = (this.mode === "scale" ? "local" : this.space) === "local" ? this.worldQuaternion : Es;
    this.gizmo.translate.visible = this.mode === "translate", this.gizmo.rotate.visible = this.mode === "rotate", this.gizmo.scale.visible = this.mode === "scale", this.helper.translate.visible = this.mode === "translate", this.helper.rotate.visible = this.mode === "rotate", this.helper.scale.visible = this.mode === "scale";
    let s = [];
    s = s.concat(this.picker[this.mode].children), s = s.concat(this.gizmo[this.mode].children), s = s.concat(this.helper[this.mode].children);
    for (let n = 0; n < s.length; n++) {
      const r = s[n];
      r.visible = !0, r.rotation.set(0, 0, 0), r.position.copy(this.worldPosition);
      let o;
      if (this.camera.isOrthographicCamera ? o = (this.camera.top - this.camera.bottom) / this.camera.zoom : o = this.worldPosition.distanceTo(this.cameraPosition) * Math.min(1.9 * Math.tan(Math.PI * this.camera.fov / 360) / this.camera.zoom, 7), r.scale.set(1, 1, 1).multiplyScalar(o * this.size / 4), r.tag === "helper") {
        r.visible = !1, r.name === "AXIS" ? (r.visible = !!this.axis, this.axis === "X" && (Ot.setFromEuler(cs.set(0, 0, 0)), r.quaternion.copy(e).multiply(Ot), Math.abs(mt.copy(Pi).applyQuaternion(e).dot(this.eye)) > 0.9 && (r.visible = !1)), this.axis === "Y" && (Ot.setFromEuler(cs.set(0, 0, Math.PI / 2)), r.quaternion.copy(e).multiply(Ot), Math.abs(mt.copy(Be).applyQuaternion(e).dot(this.eye)) > 0.9 && (r.visible = !1)), this.axis === "Z" && (Ot.setFromEuler(cs.set(0, Math.PI / 2, 0)), r.quaternion.copy(e).multiply(Ot), Math.abs(mt.copy(_i).applyQuaternion(e).dot(this.eye)) > 0.9 && (r.visible = !1)), this.axis === "XYZE" && (Ot.setFromEuler(cs.set(0, Math.PI / 2, 0)), mt.copy(this.rotationAxis), r.quaternion.setFromRotationMatrix(Dr.lookAt(Mr, mt, Be)), r.quaternion.multiply(Ot), r.visible = this.dragging), this.axis === "E" && (r.visible = !1)) : r.name === "START" ? (r.position.copy(this.worldPositionStart), r.visible = this.dragging) : r.name === "END" ? (r.position.copy(this.worldPosition), r.visible = this.dragging) : r.name === "DELTA" ? (r.position.copy(this.worldPositionStart), r.quaternion.copy(this.worldQuaternionStart), bt.set(1e-10, 1e-10, 1e-10).add(this.worldPositionStart).sub(this.worldPosition).multiplyScalar(-1), bt.applyQuaternion(this.worldQuaternionStart.clone().invert()), r.scale.copy(bt), r.visible = this.dragging) : (r.quaternion.copy(e), this.dragging ? r.position.copy(this.worldPositionStart) : r.position.copy(this.worldPosition), this.axis && (r.visible = this.axis.search(r.name) !== -1));
        continue;
      }
      r.quaternion.copy(e), this.mode === "translate" || this.mode === "scale" ? (r.name === "X" && Math.abs(mt.copy(Pi).applyQuaternion(e).dot(this.eye)) > 0.99 && (r.scale.set(1e-10, 1e-10, 1e-10), r.visible = !1), r.name === "Y" && Math.abs(mt.copy(Be).applyQuaternion(e).dot(this.eye)) > 0.99 && (r.scale.set(1e-10, 1e-10, 1e-10), r.visible = !1), r.name === "Z" && Math.abs(mt.copy(_i).applyQuaternion(e).dot(this.eye)) > 0.99 && (r.scale.set(1e-10, 1e-10, 1e-10), r.visible = !1), r.name === "XY" && Math.abs(mt.copy(_i).applyQuaternion(e).dot(this.eye)) < 0.2 && (r.scale.set(1e-10, 1e-10, 1e-10), r.visible = !1), r.name === "YZ" && Math.abs(mt.copy(Pi).applyQuaternion(e).dot(this.eye)) < 0.2 && (r.scale.set(1e-10, 1e-10, 1e-10), r.visible = !1), r.name === "XZ" && Math.abs(mt.copy(Be).applyQuaternion(e).dot(this.eye)) < 0.2 && (r.scale.set(1e-10, 1e-10, 1e-10), r.visible = !1)) : this.mode === "rotate" && (ls.copy(e), mt.copy(this.eye).applyQuaternion(Ot.copy(e).invert()), r.name.search("E") !== -1 && r.quaternion.setFromRotationMatrix(Dr.lookAt(this.eye, Mr, Be)), r.name === "X" && (Ot.setFromAxisAngle(Pi, Math.atan2(-mt.y, mt.z)), Ot.multiplyQuaternions(ls, Ot), r.quaternion.copy(Ot)), r.name === "Y" && (Ot.setFromAxisAngle(Be, Math.atan2(mt.x, mt.z)), Ot.multiplyQuaternions(ls, Ot), r.quaternion.copy(Ot)), r.name === "Z" && (Ot.setFromAxisAngle(_i, Math.atan2(mt.y, mt.x)), Ot.multiplyQuaternions(ls, Ot), r.quaternion.copy(Ot))), r.visible = r.visible && (r.name.indexOf("X") === -1 || this.showX), r.visible = r.visible && (r.name.indexOf("Y") === -1 || this.showY), r.visible = r.visible && (r.name.indexOf("Z") === -1 || this.showZ), r.visible = r.visible && (r.name.indexOf("E") === -1 || this.showX && this.showY && this.showZ), r.material._color = r.material._color || r.material.color.clone(), r.material._opacity = r.material._opacity || r.material.opacity, r.material.color.copy(r.material._color), r.material.opacity = r.material._opacity, this.enabled && this.axis && (r.name === this.axis || this.axis.split("").some(function(a) {
        return r.name === a;
      })) && (r.material.color.setHex(16776960), r.material.opacity = 1);
    }
    super.updateMatrixWorld(i);
  }
}
class Fc extends rt {
  constructor() {
    super(
      new Mo(1e5, 1e5, 2, 2),
      new Qr({ visible: !1, wireframe: !0, side: qr, transparent: !0, opacity: 0.1, toneMapped: !1 })
    ), this.isTransformControlsPlane = !0, this.type = "TransformControlsPlane";
  }
  updateMatrixWorld(i) {
    let t = this.space;
    switch (this.position.copy(this.worldPosition), this.mode === "scale" && (t = "local"), hs.copy(Pi).applyQuaternion(t === "local" ? this.worldQuaternion : Es), Oi.copy(Be).applyQuaternion(t === "local" ? this.worldQuaternion : Es), Ni.copy(_i).applyQuaternion(t === "local" ? this.worldQuaternion : Es), mt.copy(Oi), this.mode) {
      case "translate":
      case "scale":
        switch (this.axis) {
          case "X":
            mt.copy(this.eye).cross(hs), ue.copy(hs).cross(mt);
            break;
          case "Y":
            mt.copy(this.eye).cross(Oi), ue.copy(Oi).cross(mt);
            break;
          case "Z":
            mt.copy(this.eye).cross(Ni), ue.copy(Ni).cross(mt);
            break;
          case "XY":
            ue.copy(Ni);
            break;
          case "YZ":
            ue.copy(hs);
            break;
          case "XZ":
            mt.copy(Ni), ue.copy(Oi);
            break;
          case "XYZ":
          case "E":
            ue.set(0, 0, 0);
            break;
        }
        break;
      case "rotate":
      default:
        ue.set(0, 0, 0);
    }
    ue.length() === 0 ? this.quaternion.copy(this.cameraQuaternion) : (br.lookAt(bt.set(0, 0, 0), ue, mt), this.quaternion.setFromRotationMatrix(br)), super.updateMatrixWorld(i);
  }
}
class Gn {
  constructor(i, t, e, s, n, r = 5, o = !0) {
    /** Event that fires when the user starts dragging a clipping plane. */
    C(this, "onDraggingStarted", new j());
    /** Event that fires when the user stops dragging a clipping plane. */
    C(this, "onDraggingEnded", new j());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * The normal vector of the clipping plane.
     */
    C(this, "normal");
    /**
     * The origin point of the clipping plane.
     */
    C(this, "origin");
    /**
     * The THREE.js Plane object representing the clipping plane.
     */
    C(this, "three", new D.Plane());
    /** The components instance to which this plane belongs. */
    C(this, "components");
    /** The world instance to which this plane belongs. */
    C(this, "world");
    /** A custom string to identify what this plane is used for. */
    C(this, "type", "default");
    C(this, "_helper");
    C(this, "_visible", !0);
    C(this, "_enabled", !0);
    C(this, "_controlsActive", !1);
    C(this, "_arrowBoundBox", new D.Mesh());
    C(this, "_planeMesh");
    C(this, "_controls");
    C(this, "_hiddenMaterial", new D.MeshBasicMaterial({
      visible: !1
    }));
    /** {@link Updateable.update} */
    C(this, "update", () => {
      this._enabled && this.three.setFromNormalAndCoplanarPoint(
        this.normal,
        this._helper.position
      );
    });
    C(this, "changeDrag", (i) => {
      this._visible = !i.value, this.preventCameraMovement(), this.notifyDraggingChanged(i);
    });
    if (this.components = i, this.world = t, !t.renderer)
      throw new Error("The given world must have a renderer!");
    this.normal = s, this.origin = e, t.renderer.setPlane(!0, this.three), this._planeMesh = Gn.newPlaneMesh(r, n), this._helper = this.newHelper(), this._controls = this.newTransformControls(), this.three.setFromNormalAndCoplanarPoint(s, e), o && this.toggleControls(!0);
  }
  /**
   * Getter for the enabled state of the clipping plane.
   * @returns {boolean} The current enabled state.
   */
  get enabled() {
    return this._enabled;
  }
  /**
   * Setter for the enabled state of the clipping plane.
   * Updates the clipping plane state in the renderer and throws an error if no renderer is found.
   * @param {boolean} state - The new enabled state.
   */
  set enabled(i) {
    if (!this.world.isDisposing) {
      if (!this.world.renderer)
        throw new Error("No renderer found for clipping plane!");
      this._enabled = i, this.world.renderer.setPlane(i, this.three);
    }
  }
  /** {@link Hideable.visible } */
  get visible() {
    return this._visible;
  }
  /** {@link Hideable.visible } */
  set visible(i) {
    this._visible = i, this._controls.visible = i, this._helper.visible = i, this.toggleControls(i);
  }
  /** The meshes used for raycasting */
  get meshes() {
    return [this._planeMesh, this._arrowBoundBox];
  }
  /** The material of the clipping plane representation. */
  get planeMaterial() {
    return this._planeMesh.material;
  }
  /** The material of the clipping plane representation. */
  set planeMaterial(i) {
    this._planeMesh.material = i;
  }
  /** The size of the clipping plane representation. */
  get size() {
    return this._planeMesh.scale.x;
  }
  /** Sets the size of the clipping plane representation. */
  set size(i) {
    this._planeMesh.scale.set(i, i, i);
  }
  /**
   * Getter for the helper object of the clipping plane.
   * The helper object is a THREE.Object3D that contains the clipping plane mesh and other related objects.
   * It is used for positioning, rotating, and scaling the clipping plane in the 3D scene.
   *
   * @returns {THREE.Object3D} The helper object of the clipping plane.
   */
  get helper() {
    return this._helper;
  }
  /**
   * Sets the clipping plane's normal and origin from the given normal and point.
   * This method resets the clipping plane's state, updates the normal and origin,
   * and positions the helper object accordingly.
   *
   * @param normal - The new normal vector for the clipping plane.
   * @param point - The new origin point for the clipping plane.
   *
   * @returns {void}
   */
  setFromNormalAndCoplanarPoint(i, t) {
    this.reset(), this.normal.equals(i) || (this.normal.copy(i), this._helper.lookAt(i)), this.origin.copy(t), this._helper.position.copy(t), this._helper.updateMatrix(), this.update();
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this._enabled = !1, this.onDraggingStarted.reset(), this.onDraggingEnded.reset(), this._helper.removeFromParent(), this.world.renderer && this.world.renderer.setPlane(!1, this.three), this._arrowBoundBox.removeFromParent(), this._arrowBoundBox.geometry.dispose(), this._planeMesh.geometry.dispose(), this._controls.removeFromParent(), this._controls.dispose(), this.onDisposed.trigger(), this.onDisposed.reset();
  }
  reset() {
    const i = new D.Vector3(1, 0, 0), t = new D.Vector3();
    this.normal.equals(i) || (this.normal.copy(i), this._helper.lookAt(i)), this.origin.copy(t), this._helper.position.copy(t), this._helper.updateMatrix();
  }
  toggleControls(i) {
    if (i) {
      if (this._controlsActive)
        return;
      this._controls.addEventListener("change", this.update), this._controls.addEventListener("dragging-changed", this.changeDrag);
    } else
      this._controls.removeEventListener("change", this.update), this._controls.removeEventListener("dragging-changed", this.changeDrag);
    this._controlsActive = i;
  }
  newTransformControls() {
    if (!this.world.renderer)
      throw new Error("No renderer found for clipping plane!");
    const i = this.world.camera.three, t = this.world.renderer.three.domElement, e = new pc(i, t);
    return this.initializeControls(e), this.world.scene.three.add(e), e;
  }
  initializeControls(i) {
    i.attach(this._helper), i.showX = !1, i.showY = !1, i.setSpace("local"), this.createArrowBoundingBox(), i.children[0].children[0].add(this._arrowBoundBox);
  }
  createArrowBoundingBox() {
    this._arrowBoundBox.geometry = new D.CylinderGeometry(0.18, 0.18, 1.2), this._arrowBoundBox.material = this._hiddenMaterial, this._arrowBoundBox.rotateX(Math.PI / 2), this._arrowBoundBox.updateMatrix(), this._arrowBoundBox.geometry.applyMatrix4(this._arrowBoundBox.matrix);
  }
  notifyDraggingChanged(i) {
    i.value ? this.onDraggingStarted.trigger() : this.onDraggingEnded.trigger();
  }
  preventCameraMovement() {
    this.world.camera.enabled = this._visible;
  }
  newHelper() {
    const i = new D.Object3D();
    return i.lookAt(this.normal), i.position.copy(this.origin), this._planeMesh.position.z += 0.01, i.add(this._planeMesh), this.world.scene.three.add(i), i;
  }
  static newPlaneMesh(i, t) {
    const e = new D.PlaneGeometry(1), s = new D.Mesh(e, t);
    return s.scale.set(i, i, i), s;
  }
}
class Sc extends Ge {
  constructor() {
    super(...arguments);
    C(this, "_config", {
      enabled: {
        value: !0,
        type: "Boolean"
      },
      visible: {
        value: !0,
        type: "Boolean"
      },
      color: {
        value: new D.Color(),
        type: "Color"
      },
      opacity: {
        type: "Number",
        interpolable: !0,
        value: 1,
        min: 0,
        max: 1
      },
      size: {
        type: "Number",
        interpolable: !0,
        value: 2,
        min: 0,
        max: 100
      }
    });
  }
  get enabled() {
    return this._config.enabled.value;
  }
  set enabled(t) {
    this._config.enabled.value = t, this._component.enabled = t;
  }
  get visible() {
    return this._config.visible.value;
  }
  set visible(t) {
    this._config.visible.value = t, this._component.visible = t;
  }
  get color() {
    return this._config.color.value;
  }
  set color(t) {
    this._config.color.value = t, this._component.material.color.copy(t);
  }
  get opacity() {
    return this._config.opacity.value;
  }
  set opacity(t) {
    this._config.opacity.value = t, this._component.material.opacity = t;
  }
  get size() {
    return this._config.size.value;
  }
  set size(t) {
    this._config.size.value = t, this._component.size = t;
  }
}
const ni = class ni extends At {
  constructor(t) {
    super(t);
    /** {@link Configurable.onSetup} */
    C(this, "onSetup", new j());
    /** Event that fires when the user starts dragging a clipping plane. */
    C(this, "onBeforeDrag", new j());
    /** Event that fires when the user stops dragging a clipping plane. */
    C(this, "onAfterDrag", new j());
    /**
     * Event that fires when the user starts creating a clipping plane.
     */
    C(this, "onBeforeCreate", new j());
    /**
     * Event that fires when the user cancels the creation of a clipping plane.
     */
    C(this, "onBeforeCancel", new j());
    /**
     * Event that fires after the user cancels the creation of a clipping plane.
     */
    C(this, "onAfterCancel", new j());
    /**
     * Event that fires when the user starts deleting a clipping plane.
     */
    C(this, "onBeforeDelete", new j());
    /**
     * Event that fires after a clipping plane has been created.
     * @param plane - The newly created clipping plane.
     */
    C(this, "onAfterCreate", new j());
    /**
     * Event that fires after a clipping plane has been deleted.
     * @param plane - The deleted clipping plane.
     */
    C(this, "onAfterDelete", new j());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /** {@link Configurable.isSetup} */
    C(this, "isSetup", !1);
    /**
     * Whether to force the clipping plane to be orthogonal in the Y direction
     * (up). This is desirable when clipping a building horizontally and a
     * clipping plane is created in its roof, which might have a slight
     * slope for draining purposes.
     */
    C(this, "orthogonalY", !1);
    /**
     * The tolerance that determines whether an almost-horizontal clipping plane
     * will be forced to be orthogonal to the Y direction. {@link orthogonalY}
     * has to be `true` for this to apply.
     */
    C(this, "toleranceOrthogonalY", 0.7);
    /**
     * The type of clipping plane to be created.
     * Default is {@link SimplePlane}.
     */
    C(this, "Type", Gn);
    /**
     * A list of all the clipping planes created by this component.
     */
    C(this, "list", []);
    /** {@link Configurable.config} */
    C(this, "config", new Sc(
      this,
      this.components,
      "Clipper",
      ni.uuid
    ));
    C(this, "_defaultConfig", {
      color: new D.Color(12255487),
      opacity: 0.2,
      size: 2
    });
    /** The material used in all the clipping planes. */
    C(this, "_material", new D.MeshBasicMaterial({
      color: 12255487,
      side: D.DoubleSide,
      transparent: !0,
      opacity: 0.2
    }));
    C(this, "_size", 5);
    C(this, "_enabled", !1);
    C(this, "_visible", !0);
    C(this, "_onStartDragging", () => {
      this.onBeforeDrag.trigger();
    });
    C(this, "_onEndDragging", () => {
      this.onAfterDrag.trigger();
    });
    this.components.add(ni.uuid, this);
  }
  /** {@link Component.enabled} */
  get enabled() {
    return this._enabled;
  }
  /** {@link Component.enabled} */
  set enabled(t) {
    this._enabled = t;
    for (const e of this.list)
      e.enabled = t;
    this.updateMaterialsAndPlanes();
  }
  /** {@link Hideable.visible } */
  get visible() {
    return this._visible;
  }
  /** {@link Hideable.visible } */
  set visible(t) {
    this._visible = t;
    for (const e of this.list)
      e.visible = t;
  }
  /** The material of the clipping plane representation. */
  get material() {
    return this._material;
  }
  /** The material of the clipping plane representation. */
  set material(t) {
    this._material = t;
    for (const e of this.list)
      e.planeMaterial = t;
  }
  /** The size of the geometric representation of the clippings planes. */
  get size() {
    return this._size;
  }
  /** The size of the geometric representation of the clippings planes. */
  set size(t) {
    this._size = t;
    for (const e of this.list)
      e.size = t;
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this._enabled = !1, this.components.get(Ye).list.delete(this.config.uuid);
    for (const e of this.list)
      e.dispose();
    this.list.length = 0, this._material.dispose(), this.onBeforeCreate.reset(), this.onBeforeCancel.reset(), this.onBeforeDelete.reset(), this.onBeforeDrag.reset(), this.onAfterCreate.reset(), this.onAfterCancel.reset(), this.onAfterDelete.reset(), this.onAfterDrag.reset(), this.onDisposed.trigger(ni.uuid), this.onDisposed.reset();
  }
  /** {@link Createable.create} */
  create(t) {
    const n = this.components.get(ci).get(t).castRay();
    return n ? this.createPlaneFromIntersection(t, n) : null;
  }
  /**
   * Creates a plane in a certain place and with a certain orientation,
   * without the need of the mouse.
   *
   * @param world - the world where this plane should be created.
   * @param normal - the orientation of the clipping plane.
   * @param point - the position of the clipping plane.
   * navigation.
   */
  createFromNormalAndCoplanarPoint(t, e, s) {
    const n = this.newPlane(t, s, e);
    return this.updateMaterialsAndPlanes(), n;
  }
  /**
   * {@link Createable.delete}
   *
   * @param world - the world where the plane to delete is.
   * @param plane - the plane to delete. If undefined, the first plane
   * found under the cursor will be deleted.
   */
  delete(t, e) {
    e || (e = this.pickPlane(t)), e && this.deletePlane(e);
  }
  /**
   * Deletes all the existing clipping planes.
   *
   * @param types - the types of planes to be deleted. If not provided, all planes will be deleted.
   */
  deleteAll(t) {
    const e = [...this.list];
    for (const s of e)
      if (!t || t.has(s.type)) {
        this.delete(s.world, s);
        const n = this.list.indexOf(s);
        n !== -1 && this.list.splice(n, 1);
      }
  }
  /** {@link Configurable.setup} */
  setup(t) {
    const e = { ...this._defaultConfig, ...t };
    this.config.color = e.color, this.config.opacity = e.opacity, this.config.size = e.size, this.isSetup = !0, this.onSetup.trigger();
  }
  deletePlane(t) {
    const e = this.list.indexOf(t);
    if (e !== -1) {
      if (this.list.splice(e, 1), !t.world.renderer)
        throw new Error("Renderer not found for this plane's world!");
      t.world.renderer.setPlane(!1, t.three), t.dispose(), this.updateMaterialsAndPlanes(), this.onAfterDelete.trigger(t);
    }
  }
  pickPlane(t) {
    const s = this.components.get(ci).get(t), n = this.getAllPlaneMeshes(), r = s.castRay(n);
    if (r) {
      const o = r.object;
      return this.list.find((a) => a.meshes.includes(o));
    }
  }
  getAllPlaneMeshes() {
    const t = [];
    for (const e of this.list)
      t.push(...e.meshes);
    return t;
  }
  createPlaneFromIntersection(t, e) {
    var a;
    if (!t.renderer)
      throw new Error("The given world must have a renderer!");
    const s = e.point.distanceTo(new D.Vector3(0, 0, 0)), n = (a = e.face) == null ? void 0 : a.normal;
    if (!s || !n)
      return null;
    const r = this.getWorldNormal(e, n), o = this.newPlane(t, e.point, r.negate());
    return o.visible = this._visible, o.size = this._size, t.renderer.setPlane(!0, o.three), this.updateMaterialsAndPlanes(), o;
  }
  getWorldNormal(t, e) {
    const s = t.object;
    let n = t.object.matrixWorld.clone();
    if (s instanceof D.InstancedMesh && t.instanceId !== void 0) {
      const l = new D.Matrix4();
      s.getMatrixAt(t.instanceId, l), n = l.multiply(n);
    }
    const o = new D.Matrix3().getNormalMatrix(n), a = e.clone().applyMatrix3(o).normalize();
    return this.normalizePlaneDirectionY(a), a;
  }
  normalizePlaneDirectionY(t) {
    this.orthogonalY && (t.y > this.toleranceOrthogonalY && (t.x = 0, t.y = 1, t.z = 0), t.y < -this.toleranceOrthogonalY && (t.x = 0, t.y = -1, t.z = 0));
  }
  newPlane(t, e, s) {
    const n = new this.Type(
      this.components,
      t,
      e,
      s,
      this._material
    );
    return n.onDraggingStarted.add(this._onStartDragging), n.onDraggingEnded.add(this._onEndDragging), this.list.push(n), this.onAfterCreate.trigger(n), n;
  }
  updateMaterialsAndPlanes() {
    const t = this.components.get(Ts);
    for (const [e, s] of t.list) {
      if (!s.renderer)
        continue;
      s.renderer.updateClippingPlanes();
      const { clippingPlanes: n } = s.renderer;
      for (const r of s.meshes)
        if (r.material)
          if (Array.isArray(r.material))
            for (const o of r.material)
              o.clippingPlanes = n;
          else
            r.material.clippingPlanes = n;
    }
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(ni, "uuid", "66290bc5-18c4-4cd1-9379-2e17a0617611");
let On = ni;
class Oc {
  constructor(i) {
    /** {@link NavigationMode.enabled} */
    C(this, "enabled", !1);
    /** {@link NavigationMode.id} */
    C(this, "id", "FirstPerson");
    this.camera = i;
  }
  /** {@link NavigationMode.set} */
  set(i) {
    if (this.enabled = i, i) {
      if (this.camera.projection.current !== "Perspective") {
        this.camera.set("Orbit");
        return;
      }
      this.setupFirstPersonCamera();
    }
  }
  setupFirstPersonCamera() {
    const i = this.camera.controls, t = new D.Vector3();
    i.distance--, i.getPosition(t), i.minDistance = 1, i.maxDistance = 1, i.distance = 1, i.moveTo(
      t.x,
      t.y,
      t.z
    ), i.truckSpeed = 50, i.mouseButtons.wheel = vt.ACTION.DOLLY, i.touches.two = vt.ACTION.TOUCH_ZOOM_TRUCK;
  }
}
class Nc {
  constructor(i) {
    /** {@link NavigationMode.enabled} */
    C(this, "enabled", !0);
    /** {@link NavigationMode.id} */
    C(this, "id", "Orbit");
    this.camera = i, this.activateOrbitControls();
  }
  /** {@link NavigationMode.set} */
  set(i) {
    this.enabled = i, i && this.activateOrbitControls();
  }
  activateOrbitControls() {
    const i = this.camera.controls;
    i.minDistance = 1, i.maxDistance = 300;
    const t = new D.Vector3();
    i.getPosition(t);
    const e = t.length();
    i.distance = e, i.truckSpeed = 2;
    const { rotation: s } = this.camera.three, n = new D.Vector3(0, 0, -1).applyEuler(s), r = t.addScaledVector(n, e);
    i.moveTo(r.x, r.y, r.z);
  }
}
class yc {
  constructor(i) {
    /** {@link NavigationMode.enabled} */
    C(this, "enabled", !1);
    /** {@link NavigationMode.id} */
    C(this, "id", "Plan");
    C(this, "mouseAction1");
    C(this, "mouseAction2");
    C(this, "mouseInitialized", !1);
    C(this, "defaultAzimuthSpeed");
    C(this, "defaultPolarSpeed");
    this.camera = i, this.defaultAzimuthSpeed = i.controls.azimuthRotateSpeed, this.defaultPolarSpeed = i.controls.polarRotateSpeed;
  }
  /** {@link NavigationMode.set} */
  set(i) {
    this.enabled = i;
    const t = this.camera.controls;
    t.azimuthRotateSpeed = i ? 0 : this.defaultAzimuthSpeed, t.polarRotateSpeed = i ? 0 : this.defaultPolarSpeed, this.mouseInitialized || (this.mouseAction1 = t.touches.one, this.mouseAction2 = t.touches.two, this.mouseInitialized = !0), i ? (t.mouseButtons.left = vt.ACTION.TRUCK, t.touches.one = vt.ACTION.TOUCH_TRUCK, t.touches.two = vt.ACTION.TOUCH_ZOOM) : (t.mouseButtons.left = vt.ACTION.ROTATE, t.touches.one = this.mouseAction1, t.touches.two = this.mouseAction2);
  }
}
class Lc {
  constructor(i) {
    /**
     * Event that fires when the {@link CameraProjection} changes.
     */
    C(this, "onChanged", new j());
    /**
     * Current projection mode of the camera.
     * Default is "Perspective".
     */
    C(this, "current", "Perspective");
    /**
     * The camera controlled by this ProjectionManager.
     * It can be either a PerspectiveCamera or an OrthographicCamera.
     */
    C(this, "camera");
    /** Match Ortho zoom with Perspective distance when changing projection mode */
    C(this, "matchOrthoDistanceEnabled", !1);
    C(this, "_component");
    C(this, "_previousDistance", -1);
    this._component = i, this.camera = i.three;
  }
  /**
   * Sets the {@link CameraProjection} of the {@link OrthoPerspectiveCamera}.
   *
   * @param projection - the new projection to set. If it is the current projection,
   * it will have no effect.
   */
  async set(i) {
    this.current !== i && (i === "Orthographic" ? this.setOrthoCamera() : await this.setPerspectiveCamera(), this.onChanged.trigger(this.camera));
  }
  /**
   * Changes the current {@link CameraProjection} from Ortographic to Perspective
   * and vice versa.
   */
  async toggle() {
    const t = this.current === "Perspective" ? "Orthographic" : "Perspective";
    await this.set(t);
  }
  setOrthoCamera() {
    if (this._component.mode === null || this._component.mode.id === "FirstPerson")
      return;
    this._previousDistance = this._component.controls.distance, this._component.controls.distance = 200;
    const i = this.getPerspectiveDims();
    if (!i)
      return;
    const { width: t, height: e } = i;
    this.setupOrthoCamera(e, t), this.camera = this._component.threeOrtho, this.current = "Orthographic";
  }
  getPerspectiveDims() {
    const i = this._component.currentWorld;
    if (!i || !i.renderer)
      return null;
    const t = new D.Vector3();
    this._component.threePersp.getWorldDirection(t);
    const e = new D.Vector3();
    this._component.controls.getTarget(e);
    const n = e.clone().sub(this._component.threePersp.position).dot(t), r = i.renderer.getSize(), o = r.x / r.y, a = this._component.threePersp, l = n * 2 * Math.atan(a.fov * (Math.PI / 180) / 2);
    return { width: l * o, height: l };
  }
  setupOrthoCamera(i, t) {
    this._component.controls.mouseButtons.wheel = vt.ACTION.ZOOM, this._component.controls.mouseButtons.middle = vt.ACTION.ZOOM;
    const e = this._component.threePersp, s = this._component.threeOrtho;
    s.zoom = 1, s.left = t / -2, s.right = t / 2, s.top = i / 2, s.bottom = i / -2, s.updateProjectionMatrix(), s.position.copy(e.position), s.quaternion.copy(e.quaternion), this._component.controls.camera = s;
  }
  getDistance() {
    const i = this._component.threePersp, t = this._component.threeOrtho;
    return (t.top - t.bottom) / t.zoom / (2 * Math.atan(i.fov * (Math.PI / 180) / 2));
  }
  async setPerspectiveCamera() {
    this._component.controls.mouseButtons.wheel = vt.ACTION.DOLLY, this._component.controls.mouseButtons.middle = vt.ACTION.DOLLY;
    const i = this._component.threePersp, t = this._component.threeOrtho;
    i.position.copy(t.position), i.quaternion.copy(t.quaternion), this._component.controls.mouseButtons.wheel = vt.ACTION.DOLLY, this.matchOrthoDistanceEnabled ? this._component.controls.distance = this.getDistance() : this._component.controls.distance = this._previousDistance, await this._component.controls.zoomTo(1), i.updateProjectionMatrix(), this._component.controls.camera = i, this.camera = i, this.current = "Perspective";
  }
}
class Pc extends xi {
  constructor(t) {
    super(t);
    /**
     * A ProjectionManager instance that manages the projection modes of the camera.
     */
    C(this, "projection");
    /**
     * A THREE.OrthographicCamera instance that represents the orthographic camera.
     * This camera is used when the projection mode is set to orthographic.
     */
    C(this, "threeOrtho");
    /**
     * A THREE.PerspectiveCamera instance that represents the perspective camera.
     * This camera is used when the projection mode is set to perspective.
     */
    C(this, "threePersp");
    C(this, "_userInputButtons", {});
    C(this, "_frustumSize", 50);
    C(this, "_navigationModes", /* @__PURE__ */ new Map());
    C(this, "_mode", null);
    C(this, "previousSize", null);
    this.threePersp = this.three, this.threeOrtho = this.newOrthoCamera(), this.projection = new Lc(this), this.onAspectUpdated.add(() => {
      this.setOrthoPerspCameraAspect();
    }), this.projection.onChanged.add(
      (e) => {
        this.three = e, this.updateAspect();
      }
    ), this.onWorldChanged.add(({ action: e }) => {
      e === "added" && (this._navigationModes.clear(), this._navigationModes.set("Orbit", new Nc(this)), this._navigationModes.set("FirstPerson", new Oc(this)), this._navigationModes.set("Plan", new yc(this)), this._mode = this._navigationModes.get("Orbit"), this.mode.set(!0, { preventTargetAdjustment: !0 }), this.currentWorld && this.currentWorld.renderer && (this.previousSize = this.currentWorld.renderer.getSize().clone()));
    });
  }
  /**
   * Getter for the current navigation mode.
   * Throws an error if the mode is not found or the camera is not initialized.
   *
   * @returns {NavigationMode} The current navigation mode.
   *
   * @throws {Error} Throws an error if the mode is not found or the camera is not initialized.
   */
  get mode() {
    if (!this._mode)
      throw new Error("Mode not found, camera not initialized");
    return this._mode;
  }
  /** {@link Disposable.dispose} */
  dispose() {
    super.dispose(), this.threeOrtho.removeFromParent();
  }
  /**
   * Sets a new {@link NavigationMode} and disables the previous one.
   *
   * @param mode - The {@link NavigationMode} to set.
   */
  set(t) {
    if (this.mode !== null && this.mode.id !== t) {
      if (this.mode.set(!1), !this._navigationModes.has(t))
        throw new Error("The specified mode does not exist!");
      this._mode = this._navigationModes.get(t), this.mode.set(!0);
    }
  }
  /**
   * Make the camera view fit all the specified meshes.
   *
   * @param meshes the meshes to fit. If it is not defined, it will
   * evaluate {@link Components.meshes}.
   * @param offset the distance to the fit object
   */
  async fit(t, e = 1.5) {
    if (!this.enabled)
      return;
    const s = Number.MAX_VALUE, n = Number.MIN_VALUE, r = new D.Vector3(s, s, s), o = new D.Vector3(n, n, n);
    for (const u of t) {
      const d = new D.Box3().setFromObject(u);
      d.min.x < r.x && (r.x = d.min.x), d.min.y < r.y && (r.y = d.min.y), d.min.z < r.z && (r.z = d.min.z), d.max.x > o.x && (o.x = d.max.x), d.max.y > o.y && (o.y = d.max.y), d.max.z > o.z && (o.z = d.max.z);
    }
    const a = new D.Box3(r, o), l = new D.Vector3();
    a.getSize(l);
    const h = new D.Vector3();
    a.getCenter(h);
    const f = Math.max(l.x, l.y, l.z) * e, I = new D.Sphere(h, f);
    await this.controls.fitToSphere(I, !0);
  }
  /**
   * Allows or prevents all user input.
   *
   * @param active - whether to enable or disable user inputs.
   */
  setUserInput(t) {
    t ? this.enableUserInput() : this.disableUserInput();
  }
  disableUserInput() {
    this._userInputButtons.left = this.controls.mouseButtons.left, this._userInputButtons.right = this.controls.mouseButtons.right, this._userInputButtons.middle = this.controls.mouseButtons.middle, this._userInputButtons.wheel = this.controls.mouseButtons.wheel, this.controls.mouseButtons.left = 0, this.controls.mouseButtons.right = 0, this.controls.mouseButtons.middle = 0, this.controls.mouseButtons.wheel = 0;
  }
  enableUserInput() {
    Object.keys(this._userInputButtons).length !== 0 && (this.controls.mouseButtons.left = this._userInputButtons.left, this.controls.mouseButtons.right = this._userInputButtons.right, this.controls.mouseButtons.middle = this._userInputButtons.middle, this.controls.mouseButtons.wheel = this._userInputButtons.wheel);
  }
  newOrthoCamera() {
    const t = window.innerWidth / window.innerHeight;
    return new D.OrthographicCamera(
      this._frustumSize * t / -2,
      this._frustumSize * t / 2,
      this._frustumSize / 2,
      this._frustumSize / -2,
      0.1,
      1e3
    );
  }
  setOrthoPerspCameraAspect() {
    if (!this.currentWorld || !this.currentWorld.renderer || !this.previousSize)
      return;
    const t = this.currentWorld.renderer.getSize(), e = this.threeOrtho.top, s = this.threeOrtho.right, n = t.y / this.previousSize.y, r = t.x / this.previousSize.x, o = e * n, a = s * r;
    this.threeOrtho.left = -a, this.threeOrtho.right = a, this.threeOrtho.top = o, this.threeOrtho.bottom = -o, this.threeOrtho.updateProjectionMatrix(), this.previousSize.copy(t);
  }
}
const vr = /* @__PURE__ */ new Map([
  [
    k.IFCRELAGGREGATES,
    {
      forRelated: "Decomposes",
      forRelating: "IsDecomposedBy"
    }
  ],
  [
    k.IFCRELASSOCIATESMATERIAL,
    {
      forRelated: "HasAssociations",
      forRelating: "AssociatedTo"
    }
  ],
  [
    k.IFCRELASSOCIATESCLASSIFICATION,
    {
      forRelated: "HasAssociations",
      forRelating: "ClassificationForObjects"
    }
  ],
  [
    k.IFCRELASSIGNSTOGROUP,
    {
      forRelated: "HasAssignments",
      forRelating: "IsGroupedBy"
    }
  ],
  [
    k.IFCRELDEFINESBYPROPERTIES,
    {
      forRelated: "IsDefinedBy",
      forRelating: "DefinesOcurrence"
    }
  ],
  [
    k.IFCRELDEFINESBYTYPE,
    {
      forRelated: "IsTypedBy",
      forRelating: "Types"
    }
  ],
  [
    k.IFCRELDEFINESBYTEMPLATE,
    {
      forRelated: "IsDefinedBy",
      forRelating: "Defines"
    }
  ],
  [
    k.IFCRELCONTAINEDINSPATIALSTRUCTURE,
    {
      forRelated: "ContainedInStructure",
      forRelating: "ContainsElements"
    }
  ],
  [
    k.IFCRELFLOWCONTROLELEMENTS,
    {
      forRelated: "AssignedToFlowElement",
      forRelating: "HasControlElements"
    }
  ],
  [
    k.IFCRELCONNECTSELEMENTS,
    {
      forRelated: "ConnectedFrom",
      forRelating: "ConnectedTo"
    }
  ],
  [
    k.IFCRELASSIGNSTOPRODUCT,
    {
      forRelated: "HasAssignments",
      forRelating: "ReferencedBy"
    }
  ],
  [
    k.IFCRELDECLARES,
    {
      forRelated: "HasContext",
      forRelating: "Declares"
    }
  ],
  [
    k.IFCRELASSIGNSTOCONTROL,
    {
      forRelated: "HasAssignments",
      forRelating: "Controls"
    }
  ],
  [
    k.IFCRELNESTS,
    {
      forRelated: "Nests",
      forRelating: "IsNestedBy"
    }
  ],
  [
    k.IFCRELASSOCIATESDOCUMENT,
    {
      forRelated: "HasAssociations",
      forRelating: "DocumentRefForObjects"
    }
  ]
]), _c = {
  103090709: "IFCPROJECT",
  4097777520: "IFCSITE",
  4031249490: "IFCBUILDING",
  3124254112: "IFCBUILDINGSTOREY",
  3856911033: "IFCSPACE",
  1674181508: "IFCANNOTATION",
  25142252: "IFCCONTROLLER",
  32344328: "IFCBOILER",
  76236018: "IFCLAMP",
  90941305: "IFCPUMP",
  177149247: "IFCAIRTERMINALBOX",
  182646315: "IFCFLOWINSTRUMENT",
  263784265: "IFCFURNISHINGELEMENT",
  264262732: "IFCELECTRICGENERATOR",
  277319702: "IFCAUDIOVISUALAPPLIANCE",
  310824031: "IFCPIPEFITTING",
  331165859: "IFCSTAIR",
  342316401: "IFCDUCTFITTING",
  377706215: "IFCMECHANICALFASTENER",
  395920057: "IFCDOOR",
  402227799: "IFCELECTRICMOTOR",
  413509423: "IFCSYSTEMFURNITUREELEMENT",
  484807127: "IFCEVAPORATOR",
  486154966: "IFCWINDOWSTANDARDCASE",
  629592764: "IFCLIGHTFIXTURE",
  630975310: "IFCUNITARYCONTROLELEMENT",
  635142910: "IFCCABLECARRIERFITTING",
  639361253: "IFCCOIL",
  647756555: "IFCFASTENER",
  707683696: "IFCFLOWSTORAGEDEVICE",
  738039164: "IFCPROTECTIVEDEVICE",
  753842376: "IFCBEAM",
  812556717: "IFCTANK",
  819412036: "IFCFILTER",
  843113511: "IFCCOLUMN",
  862014818: "IFCELECTRICDISTRIBUTIONBOARD",
  900683007: "IFCFOOTING",
  905975707: "IFCCOLUMNSTANDARDCASE",
  926996030: "IFCVOIDINGFEATURE",
  979691226: "IFCREINFORCINGBAR",
  987401354: "IFCFLOWSEGMENT",
  1003880860: "IFCELECTRICTIMECONTROL",
  1051757585: "IFCCABLEFITTING",
  1052013943: "IFCDISTRIBUTIONCHAMBERELEMENT",
  1062813311: "IFCDISTRIBUTIONCONTROLELEMENT",
  1073191201: "IFCMEMBER",
  1095909175: "IFCBUILDINGELEMENTPROXY",
  1156407060: "IFCPLATESTANDARDCASE",
  1162798199: "IFCSWITCHINGDEVICE",
  1329646415: "IFCSHADINGDEVICE",
  1335981549: "IFCDISCRETEACCESSORY",
  1360408905: "IFCDUCTSILENCER",
  1404847402: "IFCSTACKTERMINAL",
  1426591983: "IFCFIRESUPPRESSIONTERMINAL",
  1437502449: "IFCMEDICALDEVICE",
  1509553395: "IFCFURNITURE",
  1529196076: "IFCSLAB",
  1620046519: "IFCTRANSPORTELEMENT",
  1634111441: "IFCAIRTERMINAL",
  1658829314: "IFCENERGYCONVERSIONDEVICE",
  1677625105: "IFCCIVILELEMENT",
  1687234759: "IFCPILE",
  1904799276: "IFCELECTRICAPPLIANCE",
  1911478936: "IFCMEMBERSTANDARDCASE",
  1945004755: "IFCDISTRIBUTIONELEMENT",
  1973544240: "IFCCOVERING",
  1999602285: "IFCSPACEHEATER",
  2016517767: "IFCROOF",
  2056796094: "IFCAIRTOAIRHEATRECOVERY",
  2058353004: "IFCFLOWCONTROLLER",
  2068733104: "IFCHUMIDIFIER",
  2176052936: "IFCJUNCTIONBOX",
  2188021234: "IFCFLOWMETER",
  2223149337: "IFCFLOWTERMINAL",
  2262370178: "IFCRAILING",
  2272882330: "IFCCONDENSER",
  2295281155: "IFCPROTECTIVEDEVICETRIPPINGUNIT",
  2320036040: "IFCREINFORCINGMESH",
  2347447852: "IFCTENDONANCHOR",
  2391383451: "IFCVIBRATIONISOLATOR",
  2391406946: "IFCWALL",
  2474470126: "IFCMOTORCONNECTION",
  2769231204: "IFCVIRTUALELEMENT",
  2814081492: "IFCENGINE",
  2906023776: "IFCBEAMSTANDARDCASE",
  2938176219: "IFCBURNER",
  2979338954: "IFCBUILDINGELEMENTPART",
  3024970846: "IFCRAMP",
  3026737570: "IFCTUBEBUNDLE",
  3027962421: "IFCSLABSTANDARDCASE",
  3040386961: "IFCDISTRIBUTIONFLOWELEMENT",
  3053780830: "IFCSANITARYTERMINAL",
  3079942009: "IFCOPENINGSTANDARDCASE",
  3087945054: "IFCALARM",
  3101698114: "IFCSURFACEFEATURE",
  3127900445: "IFCSLABELEMENTEDCASE",
  3132237377: "IFCFLOWMOVINGDEVICE",
  3171933400: "IFCPLATE",
  3221913625: "IFCCOMMUNICATIONSAPPLIANCE",
  3242481149: "IFCDOORSTANDARDCASE",
  3283111854: "IFCRAMPFLIGHT",
  3296154744: "IFCCHIMNEY",
  3304561284: "IFCWINDOW",
  3310460725: "IFCELECTRICFLOWSTORAGEDEVICE",
  3319311131: "IFCHEATEXCHANGER",
  3415622556: "IFCFAN",
  3420628829: "IFCSOLARDEVICE",
  3493046030: "IFCGEOGRAPHICELEMENT",
  3495092785: "IFCCURTAINWALL",
  3508470533: "IFCFLOWTREATMENTDEVICE",
  3512223829: "IFCWALLSTANDARDCASE",
  3518393246: "IFCDUCTSEGMENT",
  3571504051: "IFCCOMPRESSOR",
  3588315303: "IFCOPENINGELEMENT",
  3612865200: "IFCPIPESEGMENT",
  3640358203: "IFCCOOLINGTOWER",
  3651124850: "IFCPROJECTIONELEMENT",
  3694346114: "IFCOUTLET",
  3747195512: "IFCEVAPORATIVECOOLER",
  3758799889: "IFCCABLECARRIERSEGMENT",
  3824725483: "IFCTENDON",
  3825984169: "IFCTRANSFORMER",
  3902619387: "IFCCHILLER",
  4074379575: "IFCDAMPER",
  4086658281: "IFCSENSOR",
  4123344466: "IFCELEMENTASSEMBLY",
  4136498852: "IFCCOOLEDBEAM",
  4156078855: "IFCWALLELEMENTEDCASE",
  4175244083: "IFCINTERCEPTOR",
  4207607924: "IFCVALVE",
  4217484030: "IFCCABLESEGMENT",
  4237592921: "IFCWASTETERMINAL",
  4252922144: "IFCSTAIRFLIGHT",
  4278956645: "IFCFLOWFITTING",
  4288193352: "IFCACTUATOR",
  4292641817: "IFCUNITARYEQUIPMENT",
  3009204131: "IFCGRID"
};
class gh {
  getAll(i, t) {
    const e = {}, s = Object.keys(_c).map((n) => parseInt(n, 10));
    for (let n = 0; n < s.length; n++) {
      const r = s[n], o = i.GetLineIDsWithType(t, r), a = o.size();
      for (let l = 0; l < a; l++)
        e[o.get(l)] = r;
    }
    return e;
  }
}
const ms = {
  950732822: "IFCURIREFERENCE",
  4075327185: "IFCTIME",
  1209108979: "IFCTEMPERATURERATEOFCHANGEMEASURE",
  3457685358: "IFCSOUNDPRESSURELEVELMEASURE",
  4157543285: "IFCSOUNDPOWERLEVELMEASURE",
  2798247006: "IFCPROPERTYSETDEFINITIONSET",
  1790229001: "IFCPOSITIVEINTEGER",
  525895558: "IFCNONNEGATIVELENGTHMEASURE",
  1774176899: "IFCLINEINDEX",
  1275358634: "IFCLANGUAGEID",
  2541165894: "IFCDURATION",
  3701338814: "IFCDAYINWEEKNUMBER",
  2195413836: "IFCDATETIME",
  937566702: "IFCDATE",
  1683019596: "IFCCARDINALPOINTREFERENCE",
  2314439260: "IFCBINARY",
  1500781891: "IFCAREADENSITYMEASURE",
  3683503648: "IFCARCINDEX",
  4065007721: "IFCYEARNUMBER",
  1718600412: "IFCWARPINGMOMENTMEASURE",
  51269191: "IFCWARPINGCONSTANTMEASURE",
  2593997549: "IFCVOLUMETRICFLOWRATEMEASURE",
  3458127941: "IFCVOLUMEMEASURE",
  3345633955: "IFCVAPORPERMEABILITYMEASURE",
  1278329552: "IFCTORQUEMEASURE",
  2591213694: "IFCTIMESTAMP",
  2726807636: "IFCTIMEMEASURE",
  743184107: "IFCTHERMODYNAMICTEMPERATUREMEASURE",
  2016195849: "IFCTHERMALTRANSMITTANCEMEASURE",
  857959152: "IFCTHERMALRESISTANCEMEASURE",
  2281867870: "IFCTHERMALEXPANSIONCOEFFICIENTMEASURE",
  2645777649: "IFCTHERMALCONDUCTIVITYMEASURE",
  232962298: "IFCTHERMALADMITTANCEMEASURE",
  296282323: "IFCTEXTTRANSFORMATION",
  603696268: "IFCTEXTFONTNAME",
  3490877962: "IFCTEXTDECORATION",
  1460886941: "IFCTEXTALIGNMENT",
  2801250643: "IFCTEXT",
  58845555: "IFCTEMPERATUREGRADIENTMEASURE",
  361837227: "IFCSPECULARROUGHNESS",
  2757832317: "IFCSPECULAREXPONENT",
  3477203348: "IFCSPECIFICHEATCAPACITYMEASURE",
  993287707: "IFCSOUNDPRESSUREMEASURE",
  846465480: "IFCSOUNDPOWERMEASURE",
  3471399674: "IFCSOLIDANGLEMEASURE",
  408310005: "IFCSHEARMODULUSMEASURE",
  2190458107: "IFCSECTIONALAREAINTEGRALMEASURE",
  3467162246: "IFCSECTIONMODULUSMEASURE",
  2766185779: "IFCSECONDINMINUTE",
  3211557302: "IFCROTATIONALSTIFFNESSMEASURE",
  1755127002: "IFCROTATIONALMASSMEASURE",
  2133746277: "IFCROTATIONALFREQUENCYMEASURE",
  200335297: "IFCREAL",
  96294661: "IFCRATIOMEASURE",
  3972513137: "IFCRADIOACTIVITYMEASURE",
  3665567075: "IFCPRESSUREMEASURE",
  2169031380: "IFCPRESENTABLETEXT",
  1364037233: "IFCPOWERMEASURE",
  1245737093: "IFCPOSITIVERATIOMEASURE",
  3054510233: "IFCPOSITIVEPLANEANGLEMEASURE",
  2815919920: "IFCPOSITIVELENGTHMEASURE",
  4042175685: "IFCPLANEANGLEMEASURE",
  2642773653: "IFCPLANARFORCEMEASURE",
  2260317790: "IFCPARAMETERVALUE",
  929793134: "IFCPHMEASURE",
  2395907400: "IFCNUMERICMEASURE",
  2095195183: "IFCNORMALISEDRATIOMEASURE",
  765770214: "IFCMONTHINYEARNUMBER",
  2615040989: "IFCMONETARYMEASURE",
  3114022597: "IFCMOMENTOFINERTIAMEASURE",
  1648970520: "IFCMOLECULARWEIGHTMEASURE",
  3177669450: "IFCMOISTUREDIFFUSIVITYMEASURE",
  1753493141: "IFCMODULUSOFSUBGRADEREACTIONMEASURE",
  1052454078: "IFCMODULUSOFROTATIONALSUBGRADEREACTIONMEASURE",
  2173214787: "IFCMODULUSOFLINEARSUBGRADEREACTIONMEASURE",
  3341486342: "IFCMODULUSOFELASTICITYMEASURE",
  102610177: "IFCMINUTEINHOUR",
  3531705166: "IFCMASSPERLENGTHMEASURE",
  3124614049: "IFCMASSMEASURE",
  4017473158: "IFCMASSFLOWRATEMEASURE",
  1477762836: "IFCMASSDENSITYMEASURE",
  2486716878: "IFCMAGNETICFLUXMEASURE",
  286949696: "IFCMAGNETICFLUXDENSITYMEASURE",
  151039812: "IFCLUMINOUSINTENSITYMEASURE",
  2755797622: "IFCLUMINOUSINTENSITYDISTRIBUTIONMEASURE",
  2095003142: "IFCLUMINOUSFLUXMEASURE",
  503418787: "IFCLOGICAL",
  3086160713: "IFCLINEARVELOCITYMEASURE",
  1307019551: "IFCLINEARSTIFFNESSMEASURE",
  2128979029: "IFCLINEARMOMENTMEASURE",
  191860431: "IFCLINEARFORCEMEASURE",
  1243674935: "IFCLENGTHMEASURE",
  3258342251: "IFCLABEL",
  2054016361: "IFCKINEMATICVISCOSITYMEASURE",
  3192672207: "IFCISOTHERMALMOISTURECAPACITYMEASURE",
  3686016028: "IFCIONCONCENTRATIONMEASURE",
  3809634241: "IFCINTEGERCOUNTRATEMEASURE",
  1939436016: "IFCINTEGER",
  2679005408: "IFCINDUCTANCEMEASURE",
  3358199106: "IFCILLUMINANCEMEASURE",
  983778844: "IFCIDENTIFIER",
  2589826445: "IFCHOURINDAY",
  1158859006: "IFCHEATINGVALUEMEASURE",
  3113092358: "IFCHEATFLUXDENSITYMEASURE",
  3064340077: "IFCGLOBALLYUNIQUEID",
  3044325142: "IFCFREQUENCYMEASURE",
  1361398929: "IFCFORCEMEASURE",
  2590844177: "IFCFONTWEIGHT",
  2715512545: "IFCFONTVARIANT",
  1102727119: "IFCFONTSTYLE",
  2078135608: "IFCENERGYMEASURE",
  2506197118: "IFCELECTRICVOLTAGEMEASURE",
  2951915441: "IFCELECTRICRESISTANCEMEASURE",
  3790457270: "IFCELECTRICCURRENTMEASURE",
  2093906313: "IFCELECTRICCONDUCTANCEMEASURE",
  3818826038: "IFCELECTRICCHARGEMEASURE",
  1827137117: "IFCELECTRICCAPACITANCEMEASURE",
  69416015: "IFCDYNAMICVISCOSITYMEASURE",
  524656162: "IFCDOSEEQUIVALENTMEASURE",
  4134073009: "IFCDIMENSIONCOUNT",
  1514641115: "IFCDESCRIPTIVEMEASURE",
  300323983: "IFCDAYLIGHTSAVINGHOUR",
  86635668: "IFCDAYINMONTHNUMBER",
  94842927: "IFCCURVATUREMEASURE",
  1778710042: "IFCCOUNTMEASURE",
  3238673880: "IFCCONTEXTDEPENDENTMEASURE",
  3812528620: "IFCCOMPOUNDPLANEANGLEMEASURE",
  2991860651: "IFCCOMPLEXNUMBER",
  1867003952: "IFCBOXALIGNMENT",
  2735952531: "IFCBOOLEAN",
  2650437152: "IFCAREAMEASURE",
  632304761: "IFCANGULARVELOCITYMEASURE",
  360377573: "IFCAMOUNTOFSUBSTANCEMEASURE",
  4182062534: "IFCACCELERATIONMEASURE",
  3699917729: "IFCABSORBEDDOSEMEASURE",
  1971632696: "IFCGEOSLICE",
  2680139844: "IFCGEOMODEL",
  24726584: "IFCELECTRICFLOWTREATMENTDEVICE",
  3693000487: "IFCDISTRIBUTIONBOARD",
  3460952963: "IFCCONVEYORSEGMENT",
  3999819293: "IFCCAISSONFOUNDATION",
  3314249567: "IFCBOREHOLE",
  4196446775: "IFCBEARING",
  325726236: "IFCALIGNMENT",
  3425753595: "IFCTRACKELEMENT",
  991950508: "IFCSIGNAL",
  3798194928: "IFCREINFORCEDSOIL",
  3290496277: "IFCRAIL",
  1383356374: "IFCPAVEMENT",
  2182337498: "IFCNAVIGATIONELEMENT",
  234836483: "IFCMOORINGDEVICE",
  2078563270: "IFCMOBILETELECOMMUNICATIONSAPPLIANCE",
  1638804497: "IFCLIQUIDTERMINAL",
  1154579445: "IFCLINEARPOSITIONINGELEMENT",
  2696325953: "IFCKERB",
  2713699986: "IFCGEOTECHNICALASSEMBLY",
  2142170206: "IFCELECTRICFLOWTREATMENTDEVICETYPE",
  3376911765: "IFCEARTHWORKSFILL",
  1077100507: "IFCEARTHWORKSELEMENT",
  3071239417: "IFCEARTHWORKSCUT",
  479945903: "IFCDISTRIBUTIONBOARDTYPE",
  3426335179: "IFCDEEPFOUNDATION",
  1502416096: "IFCCOURSE",
  2940368186: "IFCCONVEYORSEGMENTTYPE",
  3203706013: "IFCCAISSONFOUNDATIONTYPE",
  3862327254: "IFCBUILTSYSTEM",
  1876633798: "IFCBUILTELEMENT",
  963979645: "IFCBRIDGEPART",
  644574406: "IFCBRIDGE",
  3649138523: "IFCBEARINGTYPE",
  1662888072: "IFCALIGNMENTVERTICAL",
  317615605: "IFCALIGNMENTSEGMENT",
  1545765605: "IFCALIGNMENTHORIZONTAL",
  4266260250: "IFCALIGNMENTCANT",
  3956297820: "IFCVIBRATIONDAMPERTYPE",
  1530820697: "IFCVIBRATIONDAMPER",
  840318589: "IFCVEHICLE",
  1953115116: "IFCTRANSPORTATIONDEVICE",
  618700268: "IFCTRACKELEMENTTYPE",
  2281632017: "IFCTENDONCONDUITTYPE",
  3663046924: "IFCTENDONCONDUIT",
  42703149: "IFCSINESPIRAL",
  1894708472: "IFCSIGNALTYPE",
  3599934289: "IFCSIGNTYPE",
  33720170: "IFCSIGN",
  1027922057: "IFCSEVENTHORDERPOLYNOMIALSPIRAL",
  544395925: "IFCSEGMENTEDREFERENCECURVE",
  3649235739: "IFCSECONDORDERPOLYNOMIALSPIRAL",
  550521510: "IFCROADPART",
  146592293: "IFCROAD",
  3818125796: "IFCRELADHERESTOELEMENT",
  4021432810: "IFCREFERENT",
  1891881377: "IFCRAILWAYPART",
  3992365140: "IFCRAILWAY",
  1763565496: "IFCRAILTYPE",
  1946335990: "IFCPOSITIONINGELEMENT",
  514975943: "IFCPAVEMENTTYPE",
  506776471: "IFCNAVIGATIONELEMENTTYPE",
  710110818: "IFCMOORINGDEVICETYPE",
  1950438474: "IFCMOBILETELECOMMUNICATIONSAPPLIANCETYPE",
  976884017: "IFCMARINEPART",
  525669439: "IFCMARINEFACILITY",
  1770583370: "IFCLIQUIDTERMINALTYPE",
  2176059722: "IFCLINEARELEMENT",
  679976338: "IFCKERBTYPE",
  3948183225: "IFCIMPACTPROTECTIONDEVICETYPE",
  2568555532: "IFCIMPACTPROTECTIONDEVICE",
  2898700619: "IFCGRADIENTCURVE",
  1594536857: "IFCGEOTECHNICALSTRATUM",
  4230923436: "IFCGEOTECHNICALELEMENT",
  4228831410: "IFCFACILITYPARTCOMMON",
  1310830890: "IFCFACILITYPART",
  24185140: "IFCFACILITY",
  4234616927: "IFCDIRECTRIXDERIVEDREFERENCESWEPTAREASOLID",
  1306400036: "IFCDEEPFOUNDATIONTYPE",
  4189326743: "IFCCOURSETYPE",
  2000195564: "IFCCOSINESPIRAL",
  3497074424: "IFCCLOTHOID",
  1626504194: "IFCBUILTELEMENTTYPE",
  3651464721: "IFCVEHICLETYPE",
  1229763772: "IFCTRIANGULATEDIRREGULARNETWORK",
  3665877780: "IFCTRANSPORTATIONDEVICETYPE",
  782932809: "IFCTHIRDORDERPOLYNOMIALSPIRAL",
  2735484536: "IFCSPIRAL",
  1356537516: "IFCSECTIONEDSURFACE",
  1290935644: "IFCSECTIONEDSOLIDHORIZONTAL",
  1862484736: "IFCSECTIONEDSOLID",
  1441486842: "IFCRELPOSITIONS",
  1033248425: "IFCRELASSOCIATESPROFILEDEF",
  3381221214: "IFCPOLYNOMIALCURVE",
  2485787929: "IFCOFFSETCURVEBYDISTANCES",
  590820931: "IFCOFFSETCURVE",
  3465909080: "IFCINDEXEDPOLYGONALTEXTUREMAP",
  593015953: "IFCDIRECTRIXCURVESWEPTAREASOLID",
  4212018352: "IFCCURVESEGMENT",
  3425423356: "IFCAXIS2PLACEMENTLINEAR",
  823603102: "IFCSEGMENT",
  2165702409: "IFCPOINTBYDISTANCEEXPRESSION",
  182550632: "IFCOPENCROSSPROFILEDEF",
  388784114: "IFCLINEARPLACEMENT",
  536804194: "IFCALIGNMENTHORIZONTALSEGMENT",
  3752311538: "IFCALIGNMENTCANTSEGMENT",
  1010789467: "IFCTEXTURECOORDINATEINDICESWITHVOIDS",
  222769930: "IFCTEXTURECOORDINATEINDICES",
  2691318326: "IFCQUANTITYNUMBER",
  3633395639: "IFCALIGNMENTVERTICALSEGMENT",
  2879124712: "IFCALIGNMENTPARAMETERSEGMENT",
  25142252: "IFCCONTROLLER",
  3087945054: "IFCALARM",
  4288193352: "IFCACTUATOR",
  630975310: "IFCUNITARYCONTROLELEMENT",
  4086658281: "IFCSENSOR",
  2295281155: "IFCPROTECTIVEDEVICETRIPPINGUNIT",
  182646315: "IFCFLOWINSTRUMENT",
  1426591983: "IFCFIRESUPPRESSIONTERMINAL",
  819412036: "IFCFILTER",
  3415622556: "IFCFAN",
  1003880860: "IFCELECTRICTIMECONTROL",
  402227799: "IFCELECTRICMOTOR",
  264262732: "IFCELECTRICGENERATOR",
  3310460725: "IFCELECTRICFLOWSTORAGEDEVICE",
  862014818: "IFCELECTRICDISTRIBUTIONBOARD",
  1904799276: "IFCELECTRICAPPLIANCE",
  1360408905: "IFCDUCTSILENCER",
  3518393246: "IFCDUCTSEGMENT",
  342316401: "IFCDUCTFITTING",
  562808652: "IFCDISTRIBUTIONCIRCUIT",
  4074379575: "IFCDAMPER",
  3640358203: "IFCCOOLINGTOWER",
  4136498852: "IFCCOOLEDBEAM",
  2272882330: "IFCCONDENSER",
  3571504051: "IFCCOMPRESSOR",
  3221913625: "IFCCOMMUNICATIONSAPPLIANCE",
  639361253: "IFCCOIL",
  3902619387: "IFCCHILLER",
  4217484030: "IFCCABLESEGMENT",
  1051757585: "IFCCABLEFITTING",
  3758799889: "IFCCABLECARRIERSEGMENT",
  635142910: "IFCCABLECARRIERFITTING",
  2938176219: "IFCBURNER",
  32344328: "IFCBOILER",
  2906023776: "IFCBEAMSTANDARDCASE",
  277319702: "IFCAUDIOVISUALAPPLIANCE",
  2056796094: "IFCAIRTOAIRHEATRECOVERY",
  177149247: "IFCAIRTERMINALBOX",
  1634111441: "IFCAIRTERMINAL",
  486154966: "IFCWINDOWSTANDARDCASE",
  4237592921: "IFCWASTETERMINAL",
  4156078855: "IFCWALLELEMENTEDCASE",
  4207607924: "IFCVALVE",
  4292641817: "IFCUNITARYEQUIPMENT",
  3179687236: "IFCUNITARYCONTROLELEMENTTYPE",
  3026737570: "IFCTUBEBUNDLE",
  3825984169: "IFCTRANSFORMER",
  812556717: "IFCTANK",
  1162798199: "IFCSWITCHINGDEVICE",
  385403989: "IFCSTRUCTURALLOADCASE",
  1404847402: "IFCSTACKTERMINAL",
  1999602285: "IFCSPACEHEATER",
  3420628829: "IFCSOLARDEVICE",
  3027962421: "IFCSLABSTANDARDCASE",
  3127900445: "IFCSLABELEMENTEDCASE",
  1329646415: "IFCSHADINGDEVICE",
  3053780830: "IFCSANITARYTERMINAL",
  2572171363: "IFCREINFORCINGBARTYPE",
  1232101972: "IFCRATIONALBSPLINECURVEWITHKNOTS",
  90941305: "IFCPUMP",
  655969474: "IFCPROTECTIVEDEVICETRIPPINGUNITTYPE",
  738039164: "IFCPROTECTIVEDEVICE",
  1156407060: "IFCPLATESTANDARDCASE",
  3612865200: "IFCPIPESEGMENT",
  310824031: "IFCPIPEFITTING",
  3694346114: "IFCOUTLET",
  144952367: "IFCOUTERBOUNDARYCURVE",
  2474470126: "IFCMOTORCONNECTION",
  1911478936: "IFCMEMBERSTANDARDCASE",
  1437502449: "IFCMEDICALDEVICE",
  629592764: "IFCLIGHTFIXTURE",
  76236018: "IFCLAMP",
  2176052936: "IFCJUNCTIONBOX",
  4175244083: "IFCINTERCEPTOR",
  2068733104: "IFCHUMIDIFIER",
  3319311131: "IFCHEATEXCHANGER",
  2188021234: "IFCFLOWMETER",
  1209101575: "IFCEXTERNALSPATIALELEMENT",
  484807127: "IFCEVAPORATOR",
  3747195512: "IFCEVAPORATIVECOOLER",
  2814081492: "IFCENGINE",
  2417008758: "IFCELECTRICDISTRIBUTIONBOARDTYPE",
  3242481149: "IFCDOORSTANDARDCASE",
  3205830791: "IFCDISTRIBUTIONSYSTEM",
  400855858: "IFCCOMMUNICATIONSAPPLIANCETYPE",
  905975707: "IFCCOLUMNSTANDARDCASE",
  1677625105: "IFCCIVILELEMENT",
  3296154744: "IFCCHIMNEY",
  2674252688: "IFCCABLEFITTINGTYPE",
  2188180465: "IFCBURNERTYPE",
  1177604601: "IFCBUILDINGSYSTEM",
  39481116: "IFCBUILDINGELEMENTPARTTYPE",
  1136057603: "IFCBOUNDARYCURVE",
  2461110595: "IFCBSPLINECURVEWITHKNOTS",
  1532957894: "IFCAUDIOVISUALAPPLIANCETYPE",
  4088093105: "IFCWORKCALENDAR",
  4009809668: "IFCWINDOWTYPE",
  926996030: "IFCVOIDINGFEATURE",
  2391383451: "IFCVIBRATIONISOLATOR",
  2415094496: "IFCTENDONTYPE",
  3081323446: "IFCTENDONANCHORTYPE",
  413509423: "IFCSYSTEMFURNITUREELEMENT",
  3101698114: "IFCSURFACEFEATURE",
  3657597509: "IFCSTRUCTURALSURFACEACTION",
  2757150158: "IFCSTRUCTURALCURVEREACTION",
  1004757350: "IFCSTRUCTURALCURVEACTION",
  338393293: "IFCSTAIRTYPE",
  1072016465: "IFCSOLARDEVICETYPE",
  4074543187: "IFCSHADINGDEVICETYPE",
  2157484638: "IFCSEAMCURVE",
  2781568857: "IFCROOFTYPE",
  2310774935: "IFCREINFORCINGMESHTYPE",
  964333572: "IFCREINFORCINGELEMENTTYPE",
  683857671: "IFCRATIONALBSPLINESURFACEWITHKNOTS",
  1469900589: "IFCRAMPTYPE",
  2839578677: "IFCPOLYGONALFACESET",
  1158309216: "IFCPILETYPE",
  3079942009: "IFCOPENINGSTANDARDCASE",
  1114901282: "IFCMEDICALDEVICETYPE",
  3113134337: "IFCINTERSECTIONCURVE",
  3946677679: "IFCINTERCEPTORTYPE",
  2571569899: "IFCINDEXEDPOLYCURVE",
  3493046030: "IFCGEOGRAPHICELEMENT",
  1509553395: "IFCFURNITURE",
  1893162501: "IFCFOOTINGTYPE",
  2853485674: "IFCEXTERNALSPATIALSTRUCTUREELEMENT",
  4148101412: "IFCEVENT",
  132023988: "IFCENGINETYPE",
  2397081782: "IFCELEMENTASSEMBLYTYPE",
  2323601079: "IFCDOORTYPE",
  1213902940: "IFCCYLINDRICALSURFACE",
  1525564444: "IFCCONSTRUCTIONPRODUCTRESOURCETYPE",
  4105962743: "IFCCONSTRUCTIONMATERIALRESOURCETYPE",
  2185764099: "IFCCONSTRUCTIONEQUIPMENTRESOURCETYPE",
  15328376: "IFCCOMPOSITECURVEONSURFACE",
  3875453745: "IFCCOMPLEXPROPERTYTEMPLATE",
  3893394355: "IFCCIVILELEMENTTYPE",
  2197970202: "IFCCHIMNEYTYPE",
  167062518: "IFCBSPLINESURFACEWITHKNOTS",
  2887950389: "IFCBSPLINESURFACE",
  2603310189: "IFCADVANCEDBREPWITHVOIDS",
  1635779807: "IFCADVANCEDBREP",
  2916149573: "IFCTRIANGULATEDFACESET",
  1935646853: "IFCTOROIDALSURFACE",
  2387106220: "IFCTESSELLATEDFACESET",
  3206491090: "IFCTASKTYPE",
  699246055: "IFCSURFACECURVE",
  4095615324: "IFCSUBCONTRACTRESOURCETYPE",
  603775116: "IFCSTRUCTURALSURFACEREACTION",
  4015995234: "IFCSPHERICALSURFACE",
  2481509218: "IFCSPATIALZONETYPE",
  463610769: "IFCSPATIALZONE",
  710998568: "IFCSPATIALELEMENTTYPE",
  1412071761: "IFCSPATIALELEMENT",
  3663146110: "IFCSIMPLEPROPERTYTEMPLATE",
  3243963512: "IFCREVOLVEDAREASOLIDTAPERED",
  816062949: "IFCREPARAMETRISEDCOMPOSITECURVESEGMENT",
  1521410863: "IFCRELSPACEBOUNDARY2NDLEVEL",
  3523091289: "IFCRELSPACEBOUNDARY1STLEVEL",
  427948657: "IFCRELINTERFERESELEMENTS",
  307848117: "IFCRELDEFINESBYTEMPLATE",
  1462361463: "IFCRELDEFINESBYOBJECT",
  2565941209: "IFCRELDECLARES",
  1027710054: "IFCRELASSIGNSTOGROUPBYFACTOR",
  3521284610: "IFCPROPERTYTEMPLATE",
  492091185: "IFCPROPERTYSETTEMPLATE",
  653396225: "IFCPROJECTLIBRARY",
  569719735: "IFCPROCEDURETYPE",
  3967405729: "IFCPREDEFINEDPROPERTYSET",
  1682466193: "IFCPCURVE",
  428585644: "IFCLABORRESOURCETYPE",
  2294589976: "IFCINDEXEDPOLYGONALFACEWITHVOIDS",
  178912537: "IFCINDEXEDPOLYGONALFACE",
  4095422895: "IFCGEOGRAPHICELEMENTTYPE",
  2652556860: "IFCFIXEDREFERENCESWEPTAREASOLID",
  2804161546: "IFCEXTRUDEDAREASOLIDTAPERED",
  4024345920: "IFCEVENTTYPE",
  2629017746: "IFCCURVEBOUNDEDSURFACE",
  1815067380: "IFCCREWRESOURCETYPE",
  3419103109: "IFCCONTEXT",
  2574617495: "IFCCONSTRUCTIONRESOURCETYPE",
  2059837836: "IFCCARTESIANPOINTLIST3D",
  1675464909: "IFCCARTESIANPOINTLIST2D",
  574549367: "IFCCARTESIANPOINTLIST",
  3406155212: "IFCADVANCEDFACE",
  3698973494: "IFCTYPERESOURCE",
  3736923433: "IFCTYPEPROCESS",
  901063453: "IFCTESSELLATEDITEM",
  1096409881: "IFCSWEPTDISKSOLIDPOLYGONAL",
  1042787934: "IFCRESOURCETIME",
  1608871552: "IFCRESOURCECONSTRAINTRELATIONSHIP",
  2943643501: "IFCRESOURCEAPPROVALRELATIONSHIP",
  2090586900: "IFCQUANTITYSET",
  1482703590: "IFCPROPERTYTEMPLATEDEFINITION",
  3778827333: "IFCPREDEFINEDPROPERTIES",
  2998442950: "IFCMIRROREDPROFILEDEF",
  853536259: "IFCMATERIALRELATIONSHIP",
  3404854881: "IFCMATERIALPROFILESETUSAGETAPERING",
  3079605661: "IFCMATERIALPROFILESETUSAGE",
  2852063980: "IFCMATERIALCONSTITUENTSET",
  3708119e3: "IFCMATERIALCONSTITUENT",
  1585845231: "IFCLAGTIME",
  2133299955: "IFCINDEXEDTRIANGLETEXTUREMAP",
  1437953363: "IFCINDEXEDTEXTUREMAP",
  3570813810: "IFCINDEXEDCOLOURMAP",
  1437805879: "IFCEXTERNALREFERENCERELATIONSHIP",
  297599258: "IFCEXTENDEDPROPERTIES",
  211053100: "IFCEVENTTIME",
  2713554722: "IFCCONVERSIONBASEDUNITWITHOFFSET",
  3285139300: "IFCCOLOURRGBLIST",
  1236880293: "IFCWORKTIME",
  1199560280: "IFCTIMEPERIOD",
  3611470254: "IFCTEXTUREVERTEXLIST",
  2771591690: "IFCTASKTIMERECURRING",
  1549132990: "IFCTASKTIME",
  2043862942: "IFCTABLECOLUMN",
  2934153892: "IFCSURFACEREINFORCEMENTAREA",
  609421318: "IFCSTRUCTURALLOADORRESULT",
  3478079324: "IFCSTRUCTURALLOADCONFIGURATION",
  1054537805: "IFCSCHEDULINGTIME",
  2439245199: "IFCRESOURCELEVELRELATIONSHIP",
  2433181523: "IFCREFERENCE",
  3915482550: "IFCRECURRENCEPATTERN",
  986844984: "IFCPROPERTYABSTRACTION",
  3843373140: "IFCPROJECTEDCRS",
  677532197: "IFCPRESENTATIONITEM",
  1507914824: "IFCMATERIALUSAGEDEFINITION",
  552965576: "IFCMATERIALPROFILEWITHOFFSETS",
  164193824: "IFCMATERIALPROFILESET",
  2235152071: "IFCMATERIALPROFILE",
  1847252529: "IFCMATERIALLAYERWITHOFFSETS",
  760658860: "IFCMATERIALDEFINITION",
  3057273783: "IFCMAPCONVERSION",
  4294318154: "IFCEXTERNALINFORMATION",
  1466758467: "IFCCOORDINATEREFERENCESYSTEM",
  1785450214: "IFCCOORDINATEOPERATION",
  775493141: "IFCCONNECTIONVOLUMEGEOMETRY",
  979691226: "IFCREINFORCINGBAR",
  3700593921: "IFCELECTRICDISTRIBUTIONPOINT",
  1062813311: "IFCDISTRIBUTIONCONTROLELEMENT",
  1052013943: "IFCDISTRIBUTIONCHAMBERELEMENT",
  578613899: "IFCCONTROLLERTYPE",
  2454782716: "IFCCHAMFEREDGEFEATURE",
  753842376: "IFCBEAM",
  3001207471: "IFCALARMTYPE",
  2874132201: "IFCACTUATORTYPE",
  3304561284: "IFCWINDOW",
  3512223829: "IFCWALLSTANDARDCASE",
  2391406946: "IFCWALL",
  3313531582: "IFCVIBRATIONISOLATORTYPE",
  2347447852: "IFCTENDONANCHOR",
  3824725483: "IFCTENDON",
  2515109513: "IFCSTRUCTURALANALYSISMODEL",
  4252922144: "IFCSTAIRFLIGHT",
  331165859: "IFCSTAIR",
  1529196076: "IFCSLAB",
  1783015770: "IFCSENSORTYPE",
  1376911519: "IFCROUNDEDEDGEFEATURE",
  2016517767: "IFCROOF",
  2320036040: "IFCREINFORCINGMESH",
  3027567501: "IFCREINFORCINGELEMENT",
  3055160366: "IFCRATIONALBEZIERCURVE",
  3283111854: "IFCRAMPFLIGHT",
  3024970846: "IFCRAMP",
  2262370178: "IFCRAILING",
  3171933400: "IFCPLATE",
  1687234759: "IFCPILE",
  1073191201: "IFCMEMBER",
  900683007: "IFCFOOTING",
  3508470533: "IFCFLOWTREATMENTDEVICE",
  2223149337: "IFCFLOWTERMINAL",
  707683696: "IFCFLOWSTORAGEDEVICE",
  987401354: "IFCFLOWSEGMENT",
  3132237377: "IFCFLOWMOVINGDEVICE",
  4037862832: "IFCFLOWINSTRUMENTTYPE",
  4278956645: "IFCFLOWFITTING",
  2058353004: "IFCFLOWCONTROLLER",
  4222183408: "IFCFIRESUPPRESSIONTERMINALTYPE",
  1810631287: "IFCFILTERTYPE",
  346874300: "IFCFANTYPE",
  1658829314: "IFCENERGYCONVERSIONDEVICE",
  857184966: "IFCELECTRICALELEMENT",
  1634875225: "IFCELECTRICALCIRCUIT",
  712377611: "IFCELECTRICTIMECONTROLTYPE",
  1217240411: "IFCELECTRICMOTORTYPE",
  1365060375: "IFCELECTRICHEATERTYPE",
  1534661035: "IFCELECTRICGENERATORTYPE",
  3277789161: "IFCELECTRICFLOWSTORAGEDEVICETYPE",
  663422040: "IFCELECTRICAPPLIANCETYPE",
  855621170: "IFCEDGEFEATURE",
  2030761528: "IFCDUCTSILENCERTYPE",
  3760055223: "IFCDUCTSEGMENTTYPE",
  869906466: "IFCDUCTFITTINGTYPE",
  395920057: "IFCDOOR",
  3041715199: "IFCDISTRIBUTIONPORT",
  3040386961: "IFCDISTRIBUTIONFLOWELEMENT",
  1945004755: "IFCDISTRIBUTIONELEMENT",
  2063403501: "IFCDISTRIBUTIONCONTROLELEMENTTYPE",
  1599208980: "IFCDISTRIBUTIONCHAMBERELEMENTTYPE",
  2635815018: "IFCDISCRETEACCESSORYTYPE",
  1335981549: "IFCDISCRETEACCESSORY",
  4147604152: "IFCDIAMETERDIMENSION",
  3961806047: "IFCDAMPERTYPE",
  3495092785: "IFCCURTAINWALL",
  1973544240: "IFCCOVERING",
  2954562838: "IFCCOOLINGTOWERTYPE",
  335055490: "IFCCOOLEDBEAMTYPE",
  488727124: "IFCCONSTRUCTIONPRODUCTRESOURCE",
  1060000209: "IFCCONSTRUCTIONMATERIALRESOURCE",
  3898045240: "IFCCONSTRUCTIONEQUIPMENTRESOURCE",
  1163958913: "IFCCONDITIONCRITERION",
  2188551683: "IFCCONDITION",
  2816379211: "IFCCONDENSERTYPE",
  3850581409: "IFCCOMPRESSORTYPE",
  843113511: "IFCCOLUMN",
  2301859152: "IFCCOILTYPE",
  2611217952: "IFCCIRCLE",
  2951183804: "IFCCHILLERTYPE",
  1285652485: "IFCCABLESEGMENTTYPE",
  3293546465: "IFCCABLECARRIERSEGMENTTYPE",
  395041908: "IFCCABLECARRIERFITTINGTYPE",
  1909888760: "IFCBUILDINGELEMENTPROXYTYPE",
  1095909175: "IFCBUILDINGELEMENTPROXY",
  2979338954: "IFCBUILDINGELEMENTPART",
  52481810: "IFCBUILDINGELEMENTCOMPONENT",
  3299480353: "IFCBUILDINGELEMENT",
  231477066: "IFCBOILERTYPE",
  1916977116: "IFCBEZIERCURVE",
  819618141: "IFCBEAMTYPE",
  1967976161: "IFCBSPLINECURVE",
  3460190687: "IFCASSET",
  2470393545: "IFCANGULARDIMENSION",
  1871374353: "IFCAIRTOAIRHEATRECOVERYTYPE",
  3352864051: "IFCAIRTERMINALTYPE",
  1411407467: "IFCAIRTERMINALBOXTYPE",
  3821786052: "IFCACTIONREQUEST",
  1213861670: "IFC2DCOMPOSITECURVE",
  1033361043: "IFCZONE",
  3342526732: "IFCWORKSCHEDULE",
  4218914973: "IFCWORKPLAN",
  1028945134: "IFCWORKCONTROL",
  1133259667: "IFCWASTETERMINALTYPE",
  1898987631: "IFCWALLTYPE",
  2769231204: "IFCVIRTUALELEMENT",
  728799441: "IFCVALVETYPE",
  1911125066: "IFCUNITARYEQUIPMENTTYPE",
  1600972822: "IFCTUBEBUNDLETYPE",
  3593883385: "IFCTRIMMEDCURVE",
  1620046519: "IFCTRANSPORTELEMENT",
  1692211062: "IFCTRANSFORMERTYPE",
  1637806684: "IFCTIMESERIESSCHEDULE",
  5716631: "IFCTANKTYPE",
  2254336722: "IFCSYSTEM",
  2315554128: "IFCSWITCHINGDEVICETYPE",
  148013059: "IFCSUBCONTRACTRESOURCE",
  1975003073: "IFCSTRUCTURALSURFACECONNECTION",
  2986769608: "IFCSTRUCTURALRESULTGROUP",
  1235345126: "IFCSTRUCTURALPOINTREACTION",
  734778138: "IFCSTRUCTURALPOINTCONNECTION",
  2082059205: "IFCSTRUCTURALPOINTACTION",
  3987759626: "IFCSTRUCTURALPLANARACTIONVARYING",
  1621171031: "IFCSTRUCTURALPLANARACTION",
  1252848954: "IFCSTRUCTURALLOADGROUP",
  1721250024: "IFCSTRUCTURALLINEARACTIONVARYING",
  1807405624: "IFCSTRUCTURALLINEARACTION",
  2445595289: "IFCSTRUCTURALCURVEMEMBERVARYING",
  214636428: "IFCSTRUCTURALCURVEMEMBER",
  4243806635: "IFCSTRUCTURALCURVECONNECTION",
  1179482911: "IFCSTRUCTURALCONNECTION",
  682877961: "IFCSTRUCTURALACTION",
  1039846685: "IFCSTAIRFLIGHTTYPE",
  3112655638: "IFCSTACKTERMINALTYPE",
  3812236995: "IFCSPACETYPE",
  652456506: "IFCSPACEPROGRAM",
  1305183839: "IFCSPACEHEATERTYPE",
  3856911033: "IFCSPACE",
  2533589738: "IFCSLABTYPE",
  4097777520: "IFCSITE",
  4105383287: "IFCSERVICELIFE",
  3517283431: "IFCSCHEDULETIMECONTROL",
  1768891740: "IFCSANITARYTERMINALTYPE",
  2863920197: "IFCRELASSIGNSTASKS",
  160246688: "IFCRELAGGREGATES",
  2324767716: "IFCRAMPFLIGHTTYPE",
  2893384427: "IFCRAILINGTYPE",
  3248260540: "IFCRADIUSDIMENSION",
  2250791053: "IFCPUMPTYPE",
  1842657554: "IFCPROTECTIVEDEVICETYPE",
  3651124850: "IFCPROJECTIONELEMENT",
  3642467123: "IFCPROJECTORDERRECORD",
  2904328755: "IFCPROJECTORDER",
  2744685151: "IFCPROCEDURE",
  3740093272: "IFCPORT",
  3724593414: "IFCPOLYLINE",
  4017108033: "IFCPLATETYPE",
  4231323485: "IFCPIPESEGMENTTYPE",
  804291784: "IFCPIPEFITTINGTYPE",
  3327091369: "IFCPERMIT",
  2382730787: "IFCPERFORMANCEHISTORY",
  2837617999: "IFCOUTLETTYPE",
  3425660407: "IFCORDERACTION",
  3588315303: "IFCOPENINGELEMENT",
  4143007308: "IFCOCCUPANT",
  1916936684: "IFCMOVE",
  977012517: "IFCMOTORCONNECTIONTYPE",
  3181161470: "IFCMEMBERTYPE",
  2108223431: "IFCMECHANICALFASTENERTYPE",
  377706215: "IFCMECHANICALFASTENER",
  2506943328: "IFCLINEARDIMENSION",
  1161773419: "IFCLIGHTFIXTURETYPE",
  1051575348: "IFCLAMPTYPE",
  3827777499: "IFCLABORRESOURCE",
  4288270099: "IFCJUNCTIONBOXTYPE",
  2391368822: "IFCINVENTORY",
  1806887404: "IFCHUMIDIFIERTYPE",
  1251058090: "IFCHEATEXCHANGERTYPE",
  2706460486: "IFCGROUP",
  3009204131: "IFCGRID",
  200128114: "IFCGASTERMINALTYPE",
  814719939: "IFCFURNITURESTANDARD",
  263784265: "IFCFURNISHINGELEMENT",
  3009222698: "IFCFLOWTREATMENTDEVICETYPE",
  2297155007: "IFCFLOWTERMINALTYPE",
  1339347760: "IFCFLOWSTORAGEDEVICETYPE",
  1834744321: "IFCFLOWSEGMENTTYPE",
  1482959167: "IFCFLOWMOVINGDEVICETYPE",
  3815607619: "IFCFLOWMETERTYPE",
  3198132628: "IFCFLOWFITTINGTYPE",
  3907093117: "IFCFLOWCONTROLLERTYPE",
  1287392070: "IFCFEATUREELEMENTSUBTRACTION",
  2143335405: "IFCFEATUREELEMENTADDITION",
  2827207264: "IFCFEATUREELEMENT",
  2489546625: "IFCFASTENERTYPE",
  647756555: "IFCFASTENER",
  3737207727: "IFCFACETEDBREPWITHVOIDS",
  807026263: "IFCFACETEDBREP",
  3390157468: "IFCEVAPORATORTYPE",
  3174744832: "IFCEVAPORATIVECOOLERTYPE",
  3272907226: "IFCEQUIPMENTSTANDARD",
  1962604670: "IFCEQUIPMENTELEMENT",
  2107101300: "IFCENERGYCONVERSIONDEVICETYPE",
  1704287377: "IFCELLIPSE",
  2590856083: "IFCELEMENTCOMPONENTTYPE",
  1623761950: "IFCELEMENTCOMPONENT",
  4123344466: "IFCELEMENTASSEMBLY",
  1758889154: "IFCELEMENT",
  360485395: "IFCELECTRICALBASEPROPERTIES",
  3849074793: "IFCDISTRIBUTIONFLOWELEMENTTYPE",
  3256556792: "IFCDISTRIBUTIONELEMENTTYPE",
  681481545: "IFCDIMENSIONCURVEDIRECTEDCALLOUT",
  1457835157: "IFCCURTAINWALLTYPE",
  3295246426: "IFCCREWRESOURCE",
  1916426348: "IFCCOVERINGTYPE",
  1419761937: "IFCCOSTSCHEDULE",
  3895139033: "IFCCOSTITEM",
  3293443760: "IFCCONTROL",
  2559216714: "IFCCONSTRUCTIONRESOURCE",
  2510884976: "IFCCONIC",
  3732776249: "IFCCOMPOSITECURVE",
  300633059: "IFCCOLUMNTYPE",
  2937912522: "IFCCIRCLEHOLLOWPROFILEDEF",
  3124254112: "IFCBUILDINGSTOREY",
  1950629157: "IFCBUILDINGELEMENTTYPE",
  4031249490: "IFCBUILDING",
  1260505505: "IFCBOUNDEDCURVE",
  3649129432: "IFCBOOLEANCLIPPINGRESULT",
  1334484129: "IFCBLOCK",
  3207858831: "IFCASYMMETRICISHAPEPROFILEDEF",
  1674181508: "IFCANNOTATION",
  2296667514: "IFCACTOR",
  2097647324: "IFCTRANSPORTELEMENTTYPE",
  3473067441: "IFCTASK",
  1580310250: "IFCSYSTEMFURNITUREELEMENTTYPE",
  4124788165: "IFCSURFACEOFREVOLUTION",
  2809605785: "IFCSURFACEOFLINEAREXTRUSION",
  2028607225: "IFCSURFACECURVESWEPTAREASOLID",
  4070609034: "IFCSTRUCTUREDDIMENSIONCALLOUT",
  2218152070: "IFCSTRUCTURALSURFACEMEMBERVARYING",
  3979015343: "IFCSTRUCTURALSURFACEMEMBER",
  3689010777: "IFCSTRUCTURALREACTION",
  530289379: "IFCSTRUCTURALMEMBER",
  3136571912: "IFCSTRUCTURALITEM",
  3544373492: "IFCSTRUCTURALACTIVITY",
  451544542: "IFCSPHERE",
  3893378262: "IFCSPATIALSTRUCTUREELEMENTTYPE",
  2706606064: "IFCSPATIALSTRUCTUREELEMENT",
  3626867408: "IFCRIGHTCIRCULARCYLINDER",
  4158566097: "IFCRIGHTCIRCULARCONE",
  1856042241: "IFCREVOLVEDAREASOLID",
  2914609552: "IFCRESOURCE",
  1401173127: "IFCRELVOIDSELEMENT",
  3451746338: "IFCRELSPACEBOUNDARY",
  366585022: "IFCRELSERVICESBUILDINGS",
  4122056220: "IFCRELSEQUENCE",
  1058617721: "IFCRELSCHEDULESCOSTITEMS",
  1245217292: "IFCRELREFERENCEDINSPATIALSTRUCTURE",
  750771296: "IFCRELPROJECTSELEMENT",
  202636808: "IFCRELOVERRIDESPROPERTIES",
  2051452291: "IFCRELOCCUPIESSPACES",
  3268803585: "IFCRELNESTS",
  4189434867: "IFCRELINTERACTIONREQUIREMENTS",
  279856033: "IFCRELFLOWCONTROLELEMENTS",
  3940055652: "IFCRELFILLSELEMENT",
  781010003: "IFCRELDEFINESBYTYPE",
  4186316022: "IFCRELDEFINESBYPROPERTIES",
  693640335: "IFCRELDEFINES",
  2551354335: "IFCRELDECOMPOSES",
  2802773753: "IFCRELCOVERSSPACES",
  886880790: "IFCRELCOVERSBLDGELEMENTS",
  3242617779: "IFCRELCONTAINEDINSPATIALSTRUCTURE",
  3678494232: "IFCRELCONNECTSWITHREALIZINGELEMENTS",
  504942748: "IFCRELCONNECTSWITHECCENTRICITY",
  1638771189: "IFCRELCONNECTSSTRUCTURALMEMBER",
  3912681535: "IFCRELCONNECTSSTRUCTURALELEMENT",
  2127690289: "IFCRELCONNECTSSTRUCTURALACTIVITY",
  3190031847: "IFCRELCONNECTSPORTS",
  4201705270: "IFCRELCONNECTSPORTTOELEMENT",
  3945020480: "IFCRELCONNECTSPATHELEMENTS",
  1204542856: "IFCRELCONNECTSELEMENTS",
  826625072: "IFCRELCONNECTS",
  2851387026: "IFCRELASSOCIATESPROFILEPROPERTIES",
  2655215786: "IFCRELASSOCIATESMATERIAL",
  3840914261: "IFCRELASSOCIATESLIBRARY",
  982818633: "IFCRELASSOCIATESDOCUMENT",
  2728634034: "IFCRELASSOCIATESCONSTRAINT",
  919958153: "IFCRELASSOCIATESCLASSIFICATION",
  4095574036: "IFCRELASSOCIATESAPPROVAL",
  1327628568: "IFCRELASSOCIATESAPPLIEDVALUE",
  1865459582: "IFCRELASSOCIATES",
  205026976: "IFCRELASSIGNSTORESOURCE",
  3372526763: "IFCRELASSIGNSTOPROJECTORDER",
  2857406711: "IFCRELASSIGNSTOPRODUCT",
  4278684876: "IFCRELASSIGNSTOPROCESS",
  1307041759: "IFCRELASSIGNSTOGROUP",
  2495723537: "IFCRELASSIGNSTOCONTROL",
  1683148259: "IFCRELASSIGNSTOACTOR",
  3939117080: "IFCRELASSIGNS",
  3454111270: "IFCRECTANGULARTRIMMEDSURFACE",
  2798486643: "IFCRECTANGULARPYRAMID",
  2770003689: "IFCRECTANGLEHOLLOWPROFILEDEF",
  3219374653: "IFCPROXY",
  1451395588: "IFCPROPERTYSET",
  4194566429: "IFCPROJECTIONCURVE",
  103090709: "IFCPROJECT",
  4208778838: "IFCPRODUCT",
  2945172077: "IFCPROCESS",
  220341763: "IFCPLANE",
  603570806: "IFCPLANARBOX",
  3566463478: "IFCPERMEABLECOVERINGPROPERTIES",
  3505215534: "IFCOFFSETCURVE3D",
  3388369263: "IFCOFFSETCURVE2D",
  3888040117: "IFCOBJECT",
  1425443689: "IFCMANIFOLDSOLIDBREP",
  1281925730: "IFCLINE",
  572779678: "IFCLSHAPEPROFILEDEF",
  1484403080: "IFCISHAPEPROFILEDEF",
  987898635: "IFCGEOMETRICCURVESET",
  1268542332: "IFCFURNITURETYPE",
  4238390223: "IFCFURNISHINGELEMENTTYPE",
  3455213021: "IFCFLUIDFLOWPROPERTIES",
  315944413: "IFCFILLAREASTYLETILES",
  4203026998: "IFCFILLAREASTYLETILESYMBOLWITHSTYLE",
  374418227: "IFCFILLAREASTYLEHATCHING",
  2047409740: "IFCFACEBASEDSURFACEMODEL",
  477187591: "IFCEXTRUDEDAREASOLID",
  80994333: "IFCENERGYPROPERTIES",
  2835456948: "IFCELLIPSEPROFILEDEF",
  2777663545: "IFCELEMENTARYSURFACE",
  339256511: "IFCELEMENTTYPE",
  1883228015: "IFCELEMENTQUANTITY",
  1472233963: "IFCEDGELOOP",
  4006246654: "IFCDRAUGHTINGPREDEFINEDCURVEFONT",
  445594917: "IFCDRAUGHTINGPREDEFINEDCOLOUR",
  3073041342: "IFCDRAUGHTINGCALLOUT",
  526551008: "IFCDOORSTYLE",
  1714330368: "IFCDOORPANELPROPERTIES",
  2963535650: "IFCDOORLININGPROPERTIES",
  32440307: "IFCDIRECTION",
  4054601972: "IFCDIMENSIONCURVETERMINATOR",
  606661476: "IFCDIMENSIONCURVE",
  693772133: "IFCDEFINEDSYMBOL",
  2827736869: "IFCCURVEBOUNDEDPLANE",
  2601014836: "IFCCURVE",
  2147822146: "IFCCSGSOLID",
  2506170314: "IFCCSGPRIMITIVE3D",
  194851669: "IFCCRANERAILFSHAPEPROFILEDEF",
  4133800736: "IFCCRANERAILASHAPEPROFILEDEF",
  2485617015: "IFCCOMPOSITECURVESEGMENT",
  2205249479: "IFCCLOSEDSHELL",
  1383045692: "IFCCIRCLEPROFILEDEF",
  1416205885: "IFCCARTESIANTRANSFORMATIONOPERATOR3DNONUNIFORM",
  3331915920: "IFCCARTESIANTRANSFORMATIONOPERATOR3D",
  3486308946: "IFCCARTESIANTRANSFORMATIONOPERATOR2DNONUNIFORM",
  3749851601: "IFCCARTESIANTRANSFORMATIONOPERATOR2D",
  59481748: "IFCCARTESIANTRANSFORMATIONOPERATOR",
  1123145078: "IFCCARTESIANPOINT",
  2898889636: "IFCCSHAPEPROFILEDEF",
  2713105998: "IFCBOXEDHALFSPACE",
  2581212453: "IFCBOUNDINGBOX",
  4182860854: "IFCBOUNDEDSURFACE",
  2736907675: "IFCBOOLEANRESULT",
  2740243338: "IFCAXIS2PLACEMENT3D",
  3125803723: "IFCAXIS2PLACEMENT2D",
  4261334040: "IFCAXIS1PLACEMENT",
  1302238472: "IFCANNOTATIONSURFACE",
  2265737646: "IFCANNOTATIONFILLAREAOCCURRENCE",
  669184980: "IFCANNOTATIONFILLAREA",
  3288037868: "IFCANNOTATIONCURVEOCCURRENCE",
  2543172580: "IFCZSHAPEPROFILEDEF",
  1299126871: "IFCWINDOWSTYLE",
  512836454: "IFCWINDOWPANELPROPERTIES",
  336235671: "IFCWINDOWLININGPROPERTIES",
  2759199220: "IFCVERTEXLOOP",
  1417489154: "IFCVECTOR",
  427810014: "IFCUSHAPEPROFILEDEF",
  2347495698: "IFCTYPEPRODUCT",
  1628702193: "IFCTYPEOBJECT",
  1345879162: "IFCTWODIRECTIONREPEATFACTOR",
  2715220739: "IFCTRAPEZIUMPROFILEDEF",
  3124975700: "IFCTEXTLITERALWITHEXTENT",
  4282788508: "IFCTEXTLITERAL",
  3028897424: "IFCTERMINATORSYMBOL",
  3071757647: "IFCTSHAPEPROFILEDEF",
  230924584: "IFCSWEPTSURFACE",
  1260650574: "IFCSWEPTDISKSOLID",
  2247615214: "IFCSWEPTAREASOLID",
  1878645084: "IFCSURFACESTYLERENDERING",
  2513912981: "IFCSURFACE",
  2233826070: "IFCSUBEDGE",
  3653947884: "IFCSTRUCTURALSTEELPROFILEPROPERTIES",
  3843319758: "IFCSTRUCTURALPROFILEPROPERTIES",
  1190533807: "IFCSTRUCTURALLOADSINGLEFORCEWARPING",
  1597423693: "IFCSTRUCTURALLOADSINGLEFORCE",
  1973038258: "IFCSTRUCTURALLOADSINGLEDISPLACEMENTDISTORTION",
  2473145415: "IFCSTRUCTURALLOADSINGLEDISPLACEMENT",
  2668620305: "IFCSTRUCTURALLOADPLANARFORCE",
  1595516126: "IFCSTRUCTURALLOADLINEARFORCE",
  390701378: "IFCSPACETHERMALLOADPROPERTIES",
  1202362311: "IFCSOUNDVALUE",
  2485662743: "IFCSOUNDPROPERTIES",
  723233188: "IFCSOLIDMODEL",
  2609359061: "IFCSLIPPAGECONNECTIONCONDITION",
  4124623270: "IFCSHELLBASEDSURFACEMODEL",
  2411513650: "IFCSERVICELIFEFACTOR",
  1509187699: "IFCSECTIONEDSPINE",
  2778083089: "IFCROUNDEDRECTANGLEPROFILEDEF",
  478536968: "IFCRELATIONSHIP",
  3765753017: "IFCREINFORCEMENTDEFINITIONPROPERTIES",
  3413951693: "IFCREGULARTIMESERIES",
  3615266464: "IFCRECTANGLEPROFILEDEF",
  110355661: "IFCPROPERTYTABLEVALUE",
  3650150729: "IFCPROPERTYSINGLEVALUE",
  3357820518: "IFCPROPERTYSETDEFINITION",
  941946838: "IFCPROPERTYREFERENCEVALUE",
  2752243245: "IFCPROPERTYLISTVALUE",
  4166981789: "IFCPROPERTYENUMERATEDVALUE",
  1680319473: "IFCPROPERTYDEFINITION",
  871118103: "IFCPROPERTYBOUNDEDVALUE",
  673634403: "IFCPRODUCTDEFINITIONSHAPE",
  179317114: "IFCPREDEFINEDPOINTMARKERSYMBOL",
  433424934: "IFCPREDEFINEDDIMENSIONSYMBOL",
  2559016684: "IFCPREDEFINEDCURVEFONT",
  759155922: "IFCPREDEFINEDCOLOUR",
  2775532180: "IFCPOLYGONALBOUNDEDHALFSPACE",
  2924175390: "IFCPOLYLOOP",
  1423911732: "IFCPOINTONSURFACE",
  4022376103: "IFCPOINTONCURVE",
  2067069095: "IFCPOINT",
  1663979128: "IFCPLANAREXTENT",
  2004835150: "IFCPLACEMENT",
  597895409: "IFCPIXELTEXTURE",
  3021840470: "IFCPHYSICALCOMPLEXQUANTITY",
  2519244187: "IFCPATH",
  2529465313: "IFCPARAMETERIZEDPROFILEDEF",
  1029017970: "IFCORIENTEDEDGE",
  2665983363: "IFCOPENSHELL",
  2833995503: "IFCONEDIRECTIONREPEATFACTOR",
  219451334: "IFCOBJECTDEFINITION",
  1430189142: "IFCMECHANICALCONCRETEMATERIALPROPERTIES",
  2022407955: "IFCMATERIALDEFINITIONREPRESENTATION",
  2347385850: "IFCMAPPEDITEM",
  1008929658: "IFCLOOP",
  2624227202: "IFCLOCALPLACEMENT",
  3422422726: "IFCLIGHTSOURCESPOT",
  1520743889: "IFCLIGHTSOURCEPOSITIONAL",
  4266656042: "IFCLIGHTSOURCEGONIOMETRIC",
  2604431987: "IFCLIGHTSOURCEDIRECTIONAL",
  125510826: "IFCLIGHTSOURCEAMBIENT",
  1402838566: "IFCLIGHTSOURCE",
  3741457305: "IFCIRREGULARTIMESERIES",
  3905492369: "IFCIMAGETEXTURE",
  2445078500: "IFCHYGROSCOPICMATERIALPROPERTIES",
  812098782: "IFCHALFSPACESOLID",
  178086475: "IFCGRIDPLACEMENT",
  3590301190: "IFCGEOMETRICSET",
  4142052618: "IFCGEOMETRICREPRESENTATIONSUBCONTEXT",
  2453401579: "IFCGEOMETRICREPRESENTATIONITEM",
  3448662350: "IFCGEOMETRICREPRESENTATIONCONTEXT",
  1446786286: "IFCGENERALPROFILEPROPERTIES",
  803998398: "IFCGENERALMATERIALPROPERTIES",
  3857492461: "IFCFUELPROPERTIES",
  738692330: "IFCFILLAREASTYLE",
  4219587988: "IFCFAILURECONNECTIONCONDITION",
  3008276851: "IFCFACESURFACE",
  803316827: "IFCFACEOUTERBOUND",
  1809719519: "IFCFACEBOUND",
  2556980723: "IFCFACE",
  1860660968: "IFCEXTENDEDMATERIALPROPERTIES",
  476780140: "IFCEDGECURVE",
  3900360178: "IFCEDGE",
  4170525392: "IFCDRAUGHTINGPREDEFINEDTEXTFONT",
  3732053477: "IFCDOCUMENTREFERENCE",
  1694125774: "IFCDIMENSIONPAIR",
  2273265877: "IFCDIMENSIONCALLOUTRELATIONSHIP",
  3632507154: "IFCDERIVEDPROFILEDEF",
  3800577675: "IFCCURVESTYLE",
  2889183280: "IFCCONVERSIONBASEDUNIT",
  3050246964: "IFCCONTEXTDEPENDENTUNIT",
  45288368: "IFCCONNECTIONPOINTECCENTRICITY",
  1981873012: "IFCCONNECTIONCURVEGEOMETRY",
  370225590: "IFCCONNECTEDFACESET",
  1485152156: "IFCCOMPOSITEPROFILEDEF",
  2542286263: "IFCCOMPLEXPROPERTY",
  776857604: "IFCCOLOURRGB",
  647927063: "IFCCLASSIFICATIONREFERENCE",
  3150382593: "IFCCENTERLINEPROFILEDEF",
  616511568: "IFCBLOBTEXTURE",
  2705031697: "IFCARBITRARYPROFILEDEFWITHVOIDS",
  1310608509: "IFCARBITRARYOPENPROFILEDEF",
  3798115385: "IFCARBITRARYCLOSEDPROFILEDEF",
  2297822566: "IFCANNOTATIONTEXTOCCURRENCE",
  3612888222: "IFCANNOTATIONSYMBOLOCCURRENCE",
  962685235: "IFCANNOTATIONSURFACEOCCURRENCE",
  2442683028: "IFCANNOTATIONOCCURRENCE",
  1065908215: "IFCWATERPROPERTIES",
  891718957: "IFCVIRTUALGRIDINTERSECTION",
  1907098498: "IFCVERTEXPOINT",
  3304826586: "IFCVERTEXBASEDTEXTUREMAP",
  2799835756: "IFCVERTEX",
  180925521: "IFCUNITASSIGNMENT",
  1735638870: "IFCTOPOLOGYREPRESENTATION",
  1377556343: "IFCTOPOLOGICALREPRESENTATIONITEM",
  581633288: "IFCTIMESERIESVALUE",
  1718945513: "IFCTIMESERIESREFERENCERELATIONSHIP",
  3101149627: "IFCTIMESERIES",
  3317419933: "IFCTHERMALMATERIALPROPERTIES",
  1210645708: "IFCTEXTUREVERTEX",
  2552916305: "IFCTEXTUREMAP",
  1742049831: "IFCTEXTURECOORDINATEGENERATOR",
  280115917: "IFCTEXTURECOORDINATE",
  1484833681: "IFCTEXTSTYLEWITHBOXCHARACTERISTICS",
  1640371178: "IFCTEXTSTYLETEXTMODEL",
  2636378356: "IFCTEXTSTYLEFORDEFINEDFONT",
  1983826977: "IFCTEXTSTYLEFONTMODEL",
  1447204868: "IFCTEXTSTYLE",
  912023232: "IFCTELECOMADDRESS",
  531007025: "IFCTABLEROW",
  985171141: "IFCTABLE",
  1290481447: "IFCSYMBOLSTYLE",
  626085974: "IFCSURFACETEXTURE",
  1351298697: "IFCSURFACESTYLEWITHTEXTURES",
  846575682: "IFCSURFACESTYLESHADING",
  1607154358: "IFCSURFACESTYLEREFRACTION",
  3303107099: "IFCSURFACESTYLELIGHTING",
  1300840506: "IFCSURFACESTYLE",
  3049322572: "IFCSTYLEDREPRESENTATION",
  3958052878: "IFCSTYLEDITEM",
  2830218821: "IFCSTYLEMODEL",
  3408363356: "IFCSTRUCTURALLOADTEMPERATURE",
  2525727697: "IFCSTRUCTURALLOADSTATIC",
  2162789131: "IFCSTRUCTURALLOAD",
  2273995522: "IFCSTRUCTURALCONNECTIONCONDITION",
  3692461612: "IFCSIMPLEPROPERTY",
  4240577450: "IFCSHAPEREPRESENTATION",
  3982875396: "IFCSHAPEMODEL",
  867548509: "IFCSHAPEASPECT",
  4165799628: "IFCSECTIONREINFORCEMENTPROPERTIES",
  2042790032: "IFCSECTIONPROPERTIES",
  448429030: "IFCSIUNIT",
  2341007311: "IFCROOT",
  3679540991: "IFCRIBPLATEPROFILEPROPERTIES",
  1660063152: "IFCREPRESENTATIONMAP",
  3008791417: "IFCREPRESENTATIONITEM",
  3377609919: "IFCREPRESENTATIONCONTEXT",
  1076942058: "IFCREPRESENTATION",
  1222501353: "IFCRELAXATION",
  1580146022: "IFCREINFORCEMENTBARPROPERTIES",
  2692823254: "IFCREFERENCESVALUEDOCUMENT",
  825690147: "IFCQUANTITYWEIGHT",
  2405470396: "IFCQUANTITYVOLUME",
  3252649465: "IFCQUANTITYTIME",
  931644368: "IFCQUANTITYLENGTH",
  2093928680: "IFCQUANTITYCOUNT",
  2044713172: "IFCQUANTITYAREA",
  3710013099: "IFCPROPERTYENUMERATION",
  148025276: "IFCPROPERTYDEPENDENCYRELATIONSHIP",
  3896028662: "IFCPROPERTYCONSTRAINTRELATIONSHIP",
  2598011224: "IFCPROPERTY",
  2802850158: "IFCPROFILEPROPERTIES",
  3958567839: "IFCPROFILEDEF",
  2267347899: "IFCPRODUCTSOFCOMBUSTIONPROPERTIES",
  2095639259: "IFCPRODUCTREPRESENTATION",
  2417041796: "IFCPRESENTATIONSTYLEASSIGNMENT",
  3119450353: "IFCPRESENTATIONSTYLE",
  1304840413: "IFCPRESENTATIONLAYERWITHSTYLE",
  2022622350: "IFCPRESENTATIONLAYERASSIGNMENT",
  1775413392: "IFCPREDEFINEDTEXTFONT",
  3213052703: "IFCPREDEFINEDTERMINATORSYMBOL",
  990879717: "IFCPREDEFINEDSYMBOL",
  3727388367: "IFCPREDEFINEDITEM",
  3355820592: "IFCPOSTALADDRESS",
  2226359599: "IFCPHYSICALSIMPLEQUANTITY",
  2483315170: "IFCPHYSICALQUANTITY",
  101040310: "IFCPERSONANDORGANIZATION",
  2077209135: "IFCPERSON",
  1207048766: "IFCOWNERHISTORY",
  1411181986: "IFCORGANIZATIONRELATIONSHIP",
  4251960020: "IFCORGANIZATION",
  1227763645: "IFCOPTICALMATERIALPROPERTIES",
  2251480897: "IFCOBJECTIVE",
  3701648758: "IFCOBJECTPLACEMENT",
  1918398963: "IFCNAMEDUNIT",
  2706619895: "IFCMONETARYUNIT",
  3368373690: "IFCMETRIC",
  677618848: "IFCMECHANICALSTEELMATERIALPROPERTIES",
  4256014907: "IFCMECHANICALMATERIALPROPERTIES",
  2597039031: "IFCMEASUREWITHUNIT",
  3265635763: "IFCMATERIALPROPERTIES",
  2199411900: "IFCMATERIALLIST",
  1303795690: "IFCMATERIALLAYERSETUSAGE",
  3303938423: "IFCMATERIALLAYERSET",
  248100487: "IFCMATERIALLAYER",
  1847130766: "IFCMATERIALCLASSIFICATIONRELATIONSHIP",
  1838606355: "IFCMATERIAL",
  30780891: "IFCLOCALTIME",
  1566485204: "IFCLIGHTINTENSITYDISTRIBUTION",
  4162380809: "IFCLIGHTDISTRIBUTIONDATA",
  3452421091: "IFCLIBRARYREFERENCE",
  2655187982: "IFCLIBRARYINFORMATION",
  3020489413: "IFCIRREGULARTIMESERIESVALUE",
  852622518: "IFCGRIDAXIS",
  3548104201: "IFCEXTERNALLYDEFINEDTEXTFONT",
  3207319532: "IFCEXTERNALLYDEFINEDSYMBOL",
  1040185647: "IFCEXTERNALLYDEFINEDSURFACESTYLE",
  2242383968: "IFCEXTERNALLYDEFINEDHATCHSTYLE",
  3200245327: "IFCEXTERNALREFERENCE",
  1648886627: "IFCENVIRONMENTALIMPACTVALUE",
  3796139169: "IFCDRAUGHTINGCALLOUTRELATIONSHIP",
  770865208: "IFCDOCUMENTINFORMATIONRELATIONSHIP",
  1154170062: "IFCDOCUMENTINFORMATION",
  1376555844: "IFCDOCUMENTELECTRONICFORMAT",
  2949456006: "IFCDIMENSIONALEXPONENTS",
  1045800335: "IFCDERIVEDUNITELEMENT",
  1765591967: "IFCDERIVEDUNIT",
  1072939445: "IFCDATEANDTIME",
  3510044353: "IFCCURVESTYLEFONTPATTERN",
  2367409068: "IFCCURVESTYLEFONTANDSCALING",
  1105321065: "IFCCURVESTYLEFONT",
  539742890: "IFCCURRENCYRELATIONSHIP",
  602808272: "IFCCOSTVALUE",
  1065062679: "IFCCOORDINATEDUNIVERSALTIMEOFFSET",
  347226245: "IFCCONSTRAINTRELATIONSHIP",
  613356794: "IFCCONSTRAINTCLASSIFICATIONRELATIONSHIP",
  1658513725: "IFCCONSTRAINTAGGREGATIONRELATIONSHIP",
  1959218052: "IFCCONSTRAINT",
  2732653382: "IFCCONNECTIONSURFACEGEOMETRY",
  4257277454: "IFCCONNECTIONPORTGEOMETRY",
  2614616156: "IFCCONNECTIONPOINTGEOMETRY",
  2859738748: "IFCCONNECTIONGEOMETRY",
  3264961684: "IFCCOLOURSPECIFICATION",
  3639012971: "IFCCLASSIFICATIONNOTATIONFACET",
  938368621: "IFCCLASSIFICATIONNOTATION",
  1098599126: "IFCCLASSIFICATIONITEMRELATIONSHIP",
  1767535486: "IFCCLASSIFICATIONITEM",
  747523909: "IFCCLASSIFICATION",
  622194075: "IFCCALENDARDATE",
  2069777674: "IFCBOUNDARYNODECONDITIONWARPING",
  1387855156: "IFCBOUNDARYNODECONDITION",
  3367102660: "IFCBOUNDARYFACECONDITION",
  1560379544: "IFCBOUNDARYEDGECONDITION",
  4037036970: "IFCBOUNDARYCONDITION",
  3869604511: "IFCAPPROVALRELATIONSHIP",
  390851274: "IFCAPPROVALPROPERTYRELATIONSHIP",
  2080292479: "IFCAPPROVALACTORRELATIONSHIP",
  130549933: "IFCAPPROVAL",
  1110488051: "IFCAPPLIEDVALUERELATIONSHIP",
  411424972: "IFCAPPLIEDVALUE",
  639542469: "IFCAPPLICATION",
  618182010: "IFCADDRESS",
  3630933823: "IFCACTORROLE",
  599546466: "FILE_DESCRIPTION",
  1390159747: "FILE_NAME",
  1109904537: "FILE_SCHEMA"
};
class oi {
  static async getUnits(i) {
    var r, o, a;
    const { IFCUNITASSIGNMENT: t } = k, e = await i.getAllPropertiesOfType(t);
    if (!e)
      return 1;
    const s = Object.keys(e), n = e[parseInt(s[0], 10)];
    for (const l of n.Units) {
      if (l.value === void 0 || l.value === null)
        continue;
      const h = await i.getProperties(l.value);
      if (!h || !h.UnitType || !h.UnitType.value || h.UnitType.value !== "LENGTHUNIT")
        continue;
      let I = 1, u = 1;
      return h.Name.value === "METRE" && (u = 1), h.Name.value === "FOOT" && (u = 0.3048), ((r = h.Prefix) == null ? void 0 : r.value) === "MILLI" ? I = 1e-3 : ((o = h.Prefix) == null ? void 0 : o.value) === "CENTI" ? I = 0.01 : ((a = h.Prefix) == null ? void 0 : a.value) === "DECI" && (I = 0.1), u * I;
    }
    return 1;
  }
  static async findItemByGuid(i, t) {
    var s;
    const e = i.getAllPropertiesIDs();
    for (const n of e) {
      const r = await i.getProperties(n);
      if (r && ((s = r.GlobalId) == null ? void 0 : s.value) === t)
        return r;
    }
    return null;
  }
  static async getRelationMap(i, t, e) {
    var a;
    const n = e ?? (async () => {
    }), r = {}, o = i.getAllPropertiesIDs();
    for (const l of o) {
      const h = await i.getProperties(l);
      if (!h)
        continue;
      const f = h.type === t, I = Object.keys(h).find(
        (p) => p.startsWith("Relating")
      ), u = Object.keys(h).find(
        (p) => p.startsWith("Related")
      );
      if (!(f && I && u))
        continue;
      const d = await i.getProperties((a = h[I]) == null ? void 0 : a.value), E = h[u];
      if (!d || !E || !(E && Array.isArray(E)))
        continue;
      const T = E.map((p) => p.value);
      await n(d.expressID, T), r[d.expressID] = T;
    }
    return r;
  }
  static async getQsetQuantities(i, t, e) {
    const n = e ?? (() => {
    }), r = await i.getProperties(t);
    return !r || r.type !== k.IFCELEMENTQUANTITY ? null : (r.Quantities ?? [{}]).map((l) => (l.value && n(l.value), l.value)).filter((l) => l !== null);
  }
  static async getPsetProps(i, t, e) {
    const n = e ?? (() => {
    }), r = await i.getProperties(t);
    return !r || r.type !== k.IFCPROPERTYSET ? null : (r.HasProperties ?? [{}]).map((l) => (l.value && n(l.value), l.value)).filter((l) => l !== null);
  }
  static async getPsetRel(i, t) {
    var o;
    if (!await i.getProperties(t))
      return null;
    const s = await i.getAllPropertiesOfType(
      k.IFCRELDEFINESBYPROPERTIES
    );
    if (!s)
      return null;
    const n = Object.values(s);
    let r = null;
    for (const a of n)
      ((o = a.RelatingPropertyDefinition) == null ? void 0 : o.value) === t && (r = a.expressID);
    return r;
  }
  static async getQsetRel(i, t) {
    return oi.getPsetRel(i, t);
  }
  static async getEntityName(i, t) {
    var r;
    const e = await i.getProperties(t);
    if (!e)
      return { key: null, name: null };
    const s = Object.keys(e).find((o) => o.endsWith("Name")) ?? null, n = s ? (r = e[s]) == null ? void 0 : r.value : null;
    return { key: s, name: n };
  }
  static async getQuantityValue(i, t) {
    const e = await i.getProperties(t);
    if (!e)
      return { key: null, value: null };
    const s = Object.keys(e).find((r) => r.endsWith("Value")) ?? null;
    let n;
    return s === null || e[s] === void 0 || e[s] === null ? n = null : n = e[s].value, { key: s, value: n };
  }
  static isRel(i) {
    return ms[i].startsWith("IFCREL");
  }
  static async attributeExists(i, t, e) {
    const s = await i.getProperties(t);
    return s ? Object.keys(s).includes(e) : !1;
  }
  static async groupEntitiesByType(i, t) {
    var s;
    const e = /* @__PURE__ */ new Map();
    for (const n of t) {
      const r = await i.getProperties(n);
      if (!r)
        continue;
      const o = r.type;
      e.get(o) || e.set(o, /* @__PURE__ */ new Set()), (s = e.get(o)) == null || s.add(n);
    }
    return e;
  }
  // static getPropertyUnits(properties: IfcProperties, expressID: number) {
  //   const entity = properties[expressID];
  //   if (!entity) return null;
  //   const propertyInstance =
  //     entity instanceof WEBIFC.IFC2X3.IfcProperty ||
  //     entity instanceof WEBIFC.IFC4.IfcProperty ||
  //     entity instanceof WEBIFC.IFC4X3.IfcProperty;
  //   if (!propertyInstance) return null;
  //   const { key: valueKey } = IfcPropertiesUtils.getQuantityValue(
  //     properties,
  //     expressID
  //   );
  //   if (!valueKey) return null;
  //   // @ts-ignore
  //   const measureName = entity[valueKey].constructor.name as string;
  //   const isMeasureAttribute = measureName.endsWith("Measure");
  //   if (!isMeasureAttribute) return null;
  //   const measureType = measureName.slice(3, measureName.length - 7);
  //   return propertyInstance;
  // }
}
const wc = {
  IFCURIREFERENCE: "IfcUriReference",
  IFCTIME: "IfcTime",
  IFCTEMPERATURERATEOFCHANGEMEASURE: "IfcTemperatureRateOfChangeMeasure",
  IFCSOUNDPRESSURELEVELMEASURE: "IfcSoundPressureLevelMeasure",
  IFCSOUNDPOWERLEVELMEASURE: "IfcSoundPowerLevelMeasure",
  IFCPROPERTYSETDEFINITIONSET: "IfcPropertySetDefinitionSet",
  IFCPOSITIVEINTEGER: "IfcPositiveInteger",
  IFCNONNEGATIVELENGTHMEASURE: "IfcNonNegativeLengthMeasure",
  IFCLINEINDEX: "IfcLineIndex",
  IFCLANGUAGEID: "IfcLanguageId",
  IFCDURATION: "IfcDuration",
  IFCDAYINWEEKNUMBER: "IfcDayInWeekNumber",
  IFCDATETIME: "IfcDateTime",
  IFCDATE: "IfcDate",
  IFCCARDINALPOINTREFERENCE: "IfcCardinalPointReference",
  IFCBINARY: "IfcBinary",
  IFCAREADENSITYMEASURE: "IfcAreaDensityMeasure",
  IFCARCINDEX: "IfcArcIndex",
  IFCYEARNUMBER: "IfcYearNumber",
  IFCWARPINGMOMENTMEASURE: "IfcWarpingMomentMeasure",
  IFCWARPINGCONSTANTMEASURE: "IfcWarpingConstantMeasure",
  IFCVOLUMETRICFLOWRATEMEASURE: "IfcVolumetricFlowRateMeasure",
  IFCVOLUMEMEASURE: "IfcVolumeMeasure",
  IFCVAPORPERMEABILITYMEASURE: "IfcVaporPermeabilityMeasure",
  IFCTORQUEMEASURE: "IfcTorqueMeasure",
  IFCTIMESTAMP: "IfcTimestamp",
  IFCTIMEMEASURE: "IfcTimeMeasure",
  IFCTHERMODYNAMICTEMPERATUREMEASURE: "IfcThermodynamicTemperatureMeasure",
  IFCTHERMALTRANSMITTANCEMEASURE: "IfcThermalTransmittanceMeasure",
  IFCTHERMALRESISTANCEMEASURE: "IfcThermalResistanceMeasure",
  IFCTHERMALEXPANSIONCOEFFICIENTMEASURE: "IfcThermalExpansionCoefficientMeasure",
  IFCTHERMALCONDUCTIVITYMEASURE: "IfcThermalConductivityMeasure",
  IFCTHERMALADMITTANCEMEASURE: "IfcThermalAdmittanceMeasure",
  IFCTEXTTRANSFORMATION: "IfcTextTransformation",
  IFCTEXTFONTNAME: "IfcTextFontName",
  IFCTEXTDECORATION: "IfcTextDecoration",
  IFCTEXTALIGNMENT: "IfcTextAlignment",
  IFCTEXT: "IfcText",
  IFCTEMPERATUREGRADIENTMEASURE: "IfcTemperatureGradientMeasure",
  IFCSPECULARROUGHNESS: "IfcSpecularRoughness",
  IFCSPECULAREXPONENT: "IfcSpecularExponent",
  IFCSPECIFICHEATCAPACITYMEASURE: "IfcSpecificHeatCapacityMeasure",
  IFCSOUNDPRESSUREMEASURE: "IfcSoundPressureMeasure",
  IFCSOUNDPOWERMEASURE: "IfcSoundPowerMeasure",
  IFCSOLIDANGLEMEASURE: "IfcSolidAngleMeasure",
  IFCSHEARMODULUSMEASURE: "IfcShearModulusMeasure",
  IFCSECTIONALAREAINTEGRALMEASURE: "IfcSectionalAreaIntegralMeasure",
  IFCSECTIONMODULUSMEASURE: "IfcSectionModulusMeasure",
  IFCSECONDINMINUTE: "IfcSecondInMinute",
  IFCROTATIONALSTIFFNESSMEASURE: "IfcRotationalStiffnessMeasure",
  IFCROTATIONALMASSMEASURE: "IfcRotationalMassMeasure",
  IFCROTATIONALFREQUENCYMEASURE: "IfcRotationalFrequencyMeasure",
  IFCREAL: "IfcReal",
  IFCRATIOMEASURE: "IfcRatioMeasure",
  IFCRADIOACTIVITYMEASURE: "IfcRadioactivityMeasure",
  IFCPRESSUREMEASURE: "IfcPressureMeasure",
  IFCPRESENTABLETEXT: "IfcPresentableText",
  IFCPOWERMEASURE: "IfcPowerMeasure",
  IFCPOSITIVERATIOMEASURE: "IfcPositiveRatioMeasure",
  IFCPOSITIVEPLANEANGLEMEASURE: "IfcPositivePlaneAngleMeasure",
  IFCPOSITIVELENGTHMEASURE: "IfcPositiveLengthMeasure",
  IFCPLANEANGLEMEASURE: "IfcPlaneAngleMeasure",
  IFCPLANARFORCEMEASURE: "IfcPlanarForceMeasure",
  IFCPARAMETERVALUE: "IfcParameterValue",
  IFCPHMEASURE: "IfcPhMeasure",
  IFCNUMERICMEASURE: "IfcNumericMeasure",
  IFCNORMALISEDRATIOMEASURE: "IfcNormalisedRatioMeasure",
  IFCMONTHINYEARNUMBER: "IfcMonthInYearNumber",
  IFCMONETARYMEASURE: "IfcMonetaryMeasure",
  IFCMOMENTOFINERTIAMEASURE: "IfcMomentOfInertiaMeasure",
  IFCMOLECULARWEIGHTMEASURE: "IfcMolecularWeightMeasure",
  IFCMOISTUREDIFFUSIVITYMEASURE: "IfcMoistureDiffusivityMeasure",
  IFCMODULUSOFSUBGRADEREACTIONMEASURE: "IfcModulusOfSubgradeReactionMeasure",
  IFCMODULUSOFROTATIONALSUBGRADEREACTIONMEASURE: "IfcModulusOfRotationalSubgradeReactionMeasure",
  IFCMODULUSOFLINEARSUBGRADEREACTIONMEASURE: "IfcModulusOfLinearSubgradeReactionMeasure",
  IFCMODULUSOFELASTICITYMEASURE: "IfcModulusOfElasticityMeasure",
  IFCMINUTEINHOUR: "IfcMinuteInHour",
  IFCMASSPERLENGTHMEASURE: "IfcMassPerLengthMeasure",
  IFCMASSMEASURE: "IfcMassMeasure",
  IFCMASSFLOWRATEMEASURE: "IfcMassFlowRateMeasure",
  IFCMASSDENSITYMEASURE: "IfcMassDensityMeasure",
  IFCMAGNETICFLUXMEASURE: "IfcMagneticFluxMeasure",
  IFCMAGNETICFLUXDENSITYMEASURE: "IfcMagneticFluxDensityMeasure",
  IFCLUMINOUSINTENSITYMEASURE: "IfcLuminousIntensityMeasure",
  IFCLUMINOUSINTENSITYDISTRIBUTIONMEASURE: "IfcLuminousIntensityDistributionMeasure",
  IFCLUMINOUSFLUXMEASURE: "IfcLuminousFluxMeasure",
  IFCLOGICAL: "IfcLogical",
  IFCLINEARVELOCITYMEASURE: "IfcLinearVelocityMeasure",
  IFCLINEARSTIFFNESSMEASURE: "IfcLinearStiffnessMeasure",
  IFCLINEARMOMENTMEASURE: "IfcLinearMomentMeasure",
  IFCLINEARFORCEMEASURE: "IfcLinearForceMeasure",
  IFCLENGTHMEASURE: "IfcLengthMeasure",
  IFCLABEL: "IfcLabel",
  IFCKINEMATICVISCOSITYMEASURE: "IfcKinematicViscosityMeasure",
  IFCISOTHERMALMOISTURECAPACITYMEASURE: "IfcIsothermalMoistureCapacityMeasure",
  IFCIONCONCENTRATIONMEASURE: "IfcIonConcentrationMeasure",
  IFCINTEGERCOUNTRATEMEASURE: "IfcIntegerCountRateMeasure",
  IFCINTEGER: "IfcInteger",
  IFCINDUCTANCEMEASURE: "IfcInductanceMeasure",
  IFCILLUMINANCEMEASURE: "IfcIlluminanceMeasure",
  IFCIDENTIFIER: "IfcIdentifier",
  IFCHOURINDAY: "IfcHourInDay",
  IFCHEATINGVALUEMEASURE: "IfcHeatingValueMeasure",
  IFCHEATFLUXDENSITYMEASURE: "IfcHeatFluxDensityMeasure",
  IFCGLOBALLYUNIQUEID: "IfcGloballyUniqueId",
  IFCFREQUENCYMEASURE: "IfcFrequencyMeasure",
  IFCFORCEMEASURE: "IfcForceMeasure",
  IFCFONTWEIGHT: "IfcFontWeight",
  IFCFONTVARIANT: "IfcFontVariant",
  IFCFONTSTYLE: "IfcFontStyle",
  IFCENERGYMEASURE: "IfcEnergyMeasure",
  IFCELECTRICVOLTAGEMEASURE: "IfcElectricVoltageMeasure",
  IFCELECTRICRESISTANCEMEASURE: "IfcElectricResistanceMeasure",
  IFCELECTRICCURRENTMEASURE: "IfcElectricCurrentMeasure",
  IFCELECTRICCONDUCTANCEMEASURE: "IfcElectricConductanceMeasure",
  IFCELECTRICCHARGEMEASURE: "IfcElectricChargeMeasure",
  IFCELECTRICCAPACITANCEMEASURE: "IfcElectricCapacitanceMeasure",
  IFCDYNAMICVISCOSITYMEASURE: "IfcDynamicViscosityMeasure",
  IFCDOSEEQUIVALENTMEASURE: "IfcDoseEquivalentMeasure",
  IFCDIMENSIONCOUNT: "IfcDimensionCount",
  IFCDESCRIPTIVEMEASURE: "IfcDescriptiveMeasure",
  IFCDAYLIGHTSAVINGHOUR: "IfcDaylightSavingHour",
  IFCDAYINMONTHNUMBER: "IfcDayInMonthNumber",
  IFCCURVATUREMEASURE: "IfcCurvatureMeasure",
  IFCCOUNTMEASURE: "IfcCountMeasure",
  IFCCONTEXTDEPENDENTMEASURE: "IfcContextDependentMeasure",
  IFCCOMPOUNDPLANEANGLEMEASURE: "IfcCompoundPlaneAngleMeasure",
  IFCCOMPLEXNUMBER: "IfcComplexNumber",
  IFCBOXALIGNMENT: "IfcBoxAlignment",
  IFCBOOLEAN: "IfcBoolean",
  IFCAREAMEASURE: "IfcAreaMeasure",
  IFCANGULARVELOCITYMEASURE: "IfcAngularVelocityMeasure",
  IFCAMOUNTOFSUBSTANCEMEASURE: "IfcAmountOfSubstanceMeasure",
  IFCACCELERATIONMEASURE: "IfcAccelerationMeasure",
  IFCABSORBEDDOSEMEASURE: "IfcAbsorbedDoseMeasure",
  IFCGEOSLICE: "IfcGeoSlice",
  IFCGEOMODEL: "IfcGeoModel",
  IFCELECTRICFLOWTREATMENTDEVICE: "IfcElectricFlowTreatmentDevice",
  IFCDISTRIBUTIONBOARD: "IfcDistributionBoard",
  IFCCONVEYORSEGMENT: "IfcConveyorSegment",
  IFCCAISSONFOUNDATION: "IfcCaissonFoundation",
  IFCBOREHOLE: "IfcBorehole",
  IFCBEARING: "IfcBearing",
  IFCALIGNMENT: "IfcAlignment",
  IFCTRACKELEMENT: "IfcTrackElement",
  IFCSIGNAL: "IfcSignal",
  IFCREINFORCEDSOIL: "IfcReinforcedSoil",
  IFCRAIL: "IfcRail",
  IFCPAVEMENT: "IfcPavement",
  IFCNAVIGATIONELEMENT: "IfcNavigationElement",
  IFCMOORINGDEVICE: "IfcMooringDevice",
  IFCMOBILETELECOMMUNICATIONSAPPLIANCE: "IfcMobileTelecommunicationsAppliance",
  IFCLIQUIDTERMINAL: "IfcLiquidTerminal",
  IFCLINEARPOSITIONINGELEMENT: "IfcLinearPositioningElement",
  IFCKERB: "IfcKerb",
  IFCGEOTECHNICALASSEMBLY: "IfcGeotechnicalAssembly",
  IFCELECTRICFLOWTREATMENTDEVICETYPE: "IfcElectricFlowTreatmentDeviceType",
  IFCEARTHWORKSFILL: "IfcEarthworksFill",
  IFCEARTHWORKSELEMENT: "IfcEarthworksElement",
  IFCEARTHWORKSCUT: "IfcEarthworksCut",
  IFCDISTRIBUTIONBOARDTYPE: "IfcDistributionBoardType",
  IFCDEEPFOUNDATION: "IfcDeepFoundation",
  IFCCOURSE: "IfcCourse",
  IFCCONVEYORSEGMENTTYPE: "IfcConveyorSegmentType",
  IFCCAISSONFOUNDATIONTYPE: "IfcCaissonFoundationType",
  IFCBUILTSYSTEM: "IfcBuiltSystem",
  IFCBUILTELEMENT: "IfcBuiltElement",
  IFCBRIDGEPART: "IfcBridgePart",
  IFCBRIDGE: "IfcBridge",
  IFCBEARINGTYPE: "IfcBearingType",
  IFCALIGNMENTVERTICAL: "IfcAlignmentVertical",
  IFCALIGNMENTSEGMENT: "IfcAlignmentSegment",
  IFCALIGNMENTHORIZONTAL: "IfcAlignmentHorizontal",
  IFCALIGNMENTCANT: "IfcAlignmentCant",
  IFCVIBRATIONDAMPERTYPE: "IfcVibrationDamperType",
  IFCVIBRATIONDAMPER: "IfcVibrationDamper",
  IFCVEHICLE: "IfcVehicle",
  IFCTRANSPORTATIONDEVICE: "IfcTransportationDevice",
  IFCTRACKELEMENTTYPE: "IfcTrackElementType",
  IFCTENDONCONDUITTYPE: "IfcTendonConduitType",
  IFCTENDONCONDUIT: "IfcTendonConduit",
  IFCSINESPIRAL: "IfcSineSpiral",
  IFCSIGNALTYPE: "IfcSignalType",
  IFCSIGNTYPE: "IfcSignType",
  IFCSIGN: "IfcSign",
  IFCSEVENTHORDERPOLYNOMIALSPIRAL: "IfcSeventhOrderPolynomialSpiral",
  IFCSEGMENTEDREFERENCECURVE: "IfcSegmentedReferenceCurve",
  IFCSECONDORDERPOLYNOMIALSPIRAL: "IfcSecondOrderPolynomialSpiral",
  IFCROADPART: "IfcRoadPart",
  IFCROAD: "IfcRoad",
  IFCRELADHERESTOELEMENT: "IfcRelAdheresToElement",
  IFCREFERENT: "IfcReferent",
  IFCRAILWAYPART: "IfcRailwayPart",
  IFCRAILWAY: "IfcRailway",
  IFCRAILTYPE: "IfcRailType",
  IFCPOSITIONINGELEMENT: "IfcPositioningElement",
  IFCPAVEMENTTYPE: "IfcPavementType",
  IFCNAVIGATIONELEMENTTYPE: "IfcNavigationElementType",
  IFCMOORINGDEVICETYPE: "IfcMooringDeviceType",
  IFCMOBILETELECOMMUNICATIONSAPPLIANCETYPE: "IfcMobileTelecommunicationsApplianceType",
  IFCMARINEPART: "IfcMarinePart",
  IFCMARINEFACILITY: "IfcMarineFacility",
  IFCLIQUIDTERMINALTYPE: "IfcLiquidTerminalType",
  IFCLINEARELEMENT: "IfcLinearElement",
  IFCKERBTYPE: "IfcKerbType",
  IFCIMPACTPROTECTIONDEVICETYPE: "IfcImpactProtectionDeviceType",
  IFCIMPACTPROTECTIONDEVICE: "IfcImpactProtectionDevice",
  IFCGRADIENTCURVE: "IfcGradientCurve",
  IFCGEOTECHNICALSTRATUM: "IfcGeotechnicalStratum",
  IFCGEOTECHNICALELEMENT: "IfcGeotechnicalElement",
  IFCFACILITYPARTCOMMON: "IfcFacilityPartCommon",
  IFCFACILITYPART: "IfcFacilityPart",
  IFCFACILITY: "IfcFacility",
  IFCDIRECTRIXDERIVEDREFERENCESWEPTAREASOLID: "IfcDirectrixDerivedReferenceSweptAreaSolid",
  IFCDEEPFOUNDATIONTYPE: "IfcDeepFoundationType",
  IFCCOURSETYPE: "IfcCourseType",
  IFCCOSINESPIRAL: "IfcCosineSpiral",
  IFCCLOTHOID: "IfcClothoid",
  IFCBUILTELEMENTTYPE: "IfcBuiltElementType",
  IFCVEHICLETYPE: "IfcVehicleType",
  IFCTRIANGULATEDIRREGULARNETWORK: "IfcTriangulatedIrregularNetwork",
  IFCTRANSPORTATIONDEVICETYPE: "IfcTransportationDeviceType",
  IFCTHIRDORDERPOLYNOMIALSPIRAL: "IfcThirdOrderPolynomialSpiral",
  IFCSPIRAL: "IfcSpiral",
  IFCSECTIONEDSURFACE: "IfcSectionedSurface",
  IFCSECTIONEDSOLIDHORIZONTAL: "IfcSectionedSolidHorizontal",
  IFCSECTIONEDSOLID: "IfcSectionedSolid",
  IFCRELPOSITIONS: "IfcRelPositions",
  IFCRELASSOCIATESPROFILEDEF: "IfcRelAssociatesProfileDef",
  IFCPOLYNOMIALCURVE: "IfcPolynomialCurve",
  IFCOFFSETCURVEBYDISTANCES: "IfcOffsetCurveByDistances",
  IFCOFFSETCURVE: "IfcOffsetCurve",
  IFCINDEXEDPOLYGONALTEXTUREMAP: "IfcIndexedPolygonalTextureMap",
  IFCDIRECTRIXCURVESWEPTAREASOLID: "IfcDirectrixCurveSweptAreaSolid",
  IFCCURVESEGMENT: "IfcCurveSegment",
  IFCAXIS2PLACEMENTLINEAR: "IfcAxis2PlacementLinear",
  IFCSEGMENT: "IfcSegment",
  IFCPOINTBYDISTANCEEXPRESSION: "IfcPointByDistanceExpression",
  IFCOPENCROSSPROFILEDEF: "IfcOpenCrossProfileDef",
  IFCLINEARPLACEMENT: "IfcLinearPlacement",
  IFCALIGNMENTHORIZONTALSEGMENT: "IfcAlignmentHorizontalSegment",
  IFCALIGNMENTCANTSEGMENT: "IfcAlignmentCantSegment",
  IFCTEXTURECOORDINATEINDICESWITHVOIDS: "IfcTextureCoordinateIndicesWithVoids",
  IFCTEXTURECOORDINATEINDICES: "IfcTextureCoordinateIndices",
  IFCQUANTITYNUMBER: "IfcQuantityNumber",
  IFCALIGNMENTVERTICALSEGMENT: "IfcAlignmentVerticalSegment",
  IFCALIGNMENTPARAMETERSEGMENT: "IfcAlignmentParameterSegment",
  IFCCONTROLLER: "IfcController",
  IFCALARM: "IfcAlarm",
  IFCACTUATOR: "IfcActuator",
  IFCUNITARYCONTROLELEMENT: "IfcUnitaryControlElement",
  IFCSENSOR: "IfcSensor",
  IFCPROTECTIVEDEVICETRIPPINGUNIT: "IfcProtectiveDeviceTrippingUnit",
  IFCFLOWINSTRUMENT: "IfcFlowInstrument",
  IFCFIRESUPPRESSIONTERMINAL: "IfcFireSuppressionTerminal",
  IFCFILTER: "IfcFilter",
  IFCFAN: "IfcFan",
  IFCELECTRICTIMECONTROL: "IfcElectricTimeControl",
  IFCELECTRICMOTOR: "IfcElectricMotor",
  IFCELECTRICGENERATOR: "IfcElectricGenerator",
  IFCELECTRICFLOWSTORAGEDEVICE: "IfcElectricFlowStorageDevice",
  IFCELECTRICDISTRIBUTIONBOARD: "IfcElectricDistributionBoard",
  IFCELECTRICAPPLIANCE: "IfcElectricAppliance",
  IFCDUCTSILENCER: "IfcDuctSilencer",
  IFCDUCTSEGMENT: "IfcDuctSegment",
  IFCDUCTFITTING: "IfcDuctFitting",
  IFCDISTRIBUTIONCIRCUIT: "IfcDistributionCircuit",
  IFCDAMPER: "IfcDamper",
  IFCCOOLINGTOWER: "IfcCoolingTower",
  IFCCOOLEDBEAM: "IfcCooledBeam",
  IFCCONDENSER: "IfcCondenser",
  IFCCOMPRESSOR: "IfcCompressor",
  IFCCOMMUNICATIONSAPPLIANCE: "IfcCommunicationsAppliance",
  IFCCOIL: "IfcCoil",
  IFCCHILLER: "IfcChiller",
  IFCCABLESEGMENT: "IfcCableSegment",
  IFCCABLEFITTING: "IfcCableFitting",
  IFCCABLECARRIERSEGMENT: "IfcCableCarrierSegment",
  IFCCABLECARRIERFITTING: "IfcCableCarrierFitting",
  IFCBURNER: "IfcBurner",
  IFCBOILER: "IfcBoiler",
  IFCBEAMSTANDARDCASE: "IfcBeamStandardCase",
  IFCAUDIOVISUALAPPLIANCE: "IfcAudioVisualAppliance",
  IFCAIRTOAIRHEATRECOVERY: "IfcAirToAirHeatRecovery",
  IFCAIRTERMINALBOX: "IfcAirTerminalBox",
  IFCAIRTERMINAL: "IfcAirTerminal",
  IFCWINDOWSTANDARDCASE: "IfcWindowStandardCase",
  IFCWASTETERMINAL: "IfcWasteTerminal",
  IFCWALLELEMENTEDCASE: "IfcWallElementedCase",
  IFCVALVE: "IfcValve",
  IFCUNITARYEQUIPMENT: "IfcUnitaryEquipment",
  IFCUNITARYCONTROLELEMENTTYPE: "IfcUnitaryControlElementType",
  IFCTUBEBUNDLE: "IfcTubeBundle",
  IFCTRANSFORMER: "IfcTransformer",
  IFCTANK: "IfcTank",
  IFCSWITCHINGDEVICE: "IfcSwitchingDevice",
  IFCSTRUCTURALLOADCASE: "IfcStructuralLoadCase",
  IFCSTACKTERMINAL: "IfcStackTerminal",
  IFCSPACEHEATER: "IfcSpaceHeater",
  IFCSOLARDEVICE: "IfcSolarDevice",
  IFCSLABSTANDARDCASE: "IfcSlabStandardCase",
  IFCSLABELEMENTEDCASE: "IfcSlabElementedCase",
  IFCSHADINGDEVICE: "IfcShadingDevice",
  IFCSANITARYTERMINAL: "IfcSanitaryTerminal",
  IFCREINFORCINGBARTYPE: "IfcReinforcingBarType",
  IFCRATIONALBSPLINECURVEWITHKNOTS: "IfcRationalBSplineCurveWithKnots",
  IFCPUMP: "IfcPump",
  IFCPROTECTIVEDEVICETRIPPINGUNITTYPE: "IfcProtectiveDeviceTrippingUnitType",
  IFCPROTECTIVEDEVICE: "IfcProtectiveDevice",
  IFCPLATESTANDARDCASE: "IfcPlateStandardCase",
  IFCPIPESEGMENT: "IfcPipeSegment",
  IFCPIPEFITTING: "IfcPipeFitting",
  IFCOUTLET: "IfcOutlet",
  IFCOUTERBOUNDARYCURVE: "IfcOuterBoundaryCurve",
  IFCMOTORCONNECTION: "IfcMotorConnection",
  IFCMEMBERSTANDARDCASE: "IfcMemberStandardCase",
  IFCMEDICALDEVICE: "IfcMedicalDevice",
  IFCLIGHTFIXTURE: "IfcLightFixture",
  IFCLAMP: "IfcLamp",
  IFCJUNCTIONBOX: "IfcJunctionBox",
  IFCINTERCEPTOR: "IfcInterceptor",
  IFCHUMIDIFIER: "IfcHumidifier",
  IFCHEATEXCHANGER: "IfcHeatExchanger",
  IFCFLOWMETER: "IfcFlowMeter",
  IFCEXTERNALSPATIALELEMENT: "IfcExternalSpatialElement",
  IFCEVAPORATOR: "IfcEvaporator",
  IFCEVAPORATIVECOOLER: "IfcEvaporativeCooler",
  IFCENGINE: "IfcEngine",
  IFCELECTRICDISTRIBUTIONBOARDTYPE: "IfcElectricDistributionBoardType",
  IFCDOORSTANDARDCASE: "IfcDoorStandardCase",
  IFCDISTRIBUTIONSYSTEM: "IfcDistributionSystem",
  IFCCOMMUNICATIONSAPPLIANCETYPE: "IfcCommunicationsApplianceType",
  IFCCOLUMNSTANDARDCASE: "IfcColumnStandardCase",
  IFCCIVILELEMENT: "IfcCivilElement",
  IFCCHIMNEY: "IfcChimney",
  IFCCABLEFITTINGTYPE: "IfcCableFittingType",
  IFCBURNERTYPE: "IfcBurnerType",
  IFCBUILDINGSYSTEM: "IfcBuildingSystem",
  IFCBUILDINGELEMENTPARTTYPE: "IfcBuildingElementPartType",
  IFCBOUNDARYCURVE: "IfcBoundaryCurve",
  IFCBSPLINECURVEWITHKNOTS: "IfcBSplineCurveWithKnots",
  IFCAUDIOVISUALAPPLIANCETYPE: "IfcAudioVisualApplianceType",
  IFCWORKCALENDAR: "IfcWorkCalendar",
  IFCWINDOWTYPE: "IfcWindowType",
  IFCVOIDINGFEATURE: "IfcVoidingFeature",
  IFCVIBRATIONISOLATOR: "IfcVibrationIsolator",
  IFCTENDONTYPE: "IfcTendonType",
  IFCTENDONANCHORTYPE: "IfcTendonAnchorType",
  IFCSYSTEMFURNITUREELEMENT: "IfcSystemFurnitureElement",
  IFCSURFACEFEATURE: "IfcSurfaceFeature",
  IFCSTRUCTURALSURFACEACTION: "IfcStructuralSurfaceAction",
  IFCSTRUCTURALCURVEREACTION: "IfcStructuralCurveReaction",
  IFCSTRUCTURALCURVEACTION: "IfcStructuralCurveAction",
  IFCSTAIRTYPE: "IfcStairType",
  IFCSOLARDEVICETYPE: "IfcSolarDeviceType",
  IFCSHADINGDEVICETYPE: "IfcShadingDeviceType",
  IFCSEAMCURVE: "IfcSeamCurve",
  IFCROOFTYPE: "IfcRoofType",
  IFCREINFORCINGMESHTYPE: "IfcReinforcingMeshType",
  IFCREINFORCINGELEMENTTYPE: "IfcReinforcingElementType",
  IFCRATIONALBSPLINESURFACEWITHKNOTS: "IfcRationalBSplineSurfaceWithKnots",
  IFCRAMPTYPE: "IfcRampType",
  IFCPOLYGONALFACESET: "IfcPolygonalFaceSet",
  IFCPILETYPE: "IfcPileType",
  IFCOPENINGSTANDARDCASE: "IfcOpeningStandardCase",
  IFCMEDICALDEVICETYPE: "IfcMedicalDeviceType",
  IFCINTERSECTIONCURVE: "IfcIntersectionCurve",
  IFCINTERCEPTORTYPE: "IfcInterceptorType",
  IFCINDEXEDPOLYCURVE: "IfcIndexedPolyCurve",
  IFCGEOGRAPHICELEMENT: "IfcGeographicElement",
  IFCFURNITURE: "IfcFurniture",
  IFCFOOTINGTYPE: "IfcFootingType",
  IFCEXTERNALSPATIALSTRUCTUREELEMENT: "IfcExternalSpatialStructureElement",
  IFCEVENT: "IfcEvent",
  IFCENGINETYPE: "IfcEngineType",
  IFCELEMENTASSEMBLYTYPE: "IfcElementAssemblyType",
  IFCDOORTYPE: "IfcDoorType",
  IFCCYLINDRICALSURFACE: "IfcCylindricalSurface",
  IFCCONSTRUCTIONPRODUCTRESOURCETYPE: "IfcConstructionProductResourceType",
  IFCCONSTRUCTIONMATERIALRESOURCETYPE: "IfcConstructionMaterialResourceType",
  IFCCONSTRUCTIONEQUIPMENTRESOURCETYPE: "IfcConstructionEquipmentResourceType",
  IFCCOMPOSITECURVEONSURFACE: "IfcCompositeCurveOnSurface",
  IFCCOMPLEXPROPERTYTEMPLATE: "IfcComplexPropertyTemplate",
  IFCCIVILELEMENTTYPE: "IfcCivilElementType",
  IFCCHIMNEYTYPE: "IfcChimneyType",
  IFCBSPLINESURFACEWITHKNOTS: "IfcBSplineSurfaceWithKnots",
  IFCBSPLINESURFACE: "IfcBSplineSurface",
  IFCADVANCEDBREPWITHVOIDS: "IfcAdvancedBrepWithVoids",
  IFCADVANCEDBREP: "IfcAdvancedBrep",
  IFCTRIANGULATEDFACESET: "IfcTriangulatedFaceSet",
  IFCTOROIDALSURFACE: "IfcToroidalSurface",
  IFCTESSELLATEDFACESET: "IfcTessellatedFaceSet",
  IFCTASKTYPE: "IfcTaskType",
  IFCSURFACECURVE: "IfcSurfaceCurve",
  IFCSUBCONTRACTRESOURCETYPE: "IfcSubContractResourceType",
  IFCSTRUCTURALSURFACEREACTION: "IfcStructuralSurfaceReaction",
  IFCSPHERICALSURFACE: "IfcSphericalSurface",
  IFCSPATIALZONETYPE: "IfcSpatialZoneType",
  IFCSPATIALZONE: "IfcSpatialZone",
  IFCSPATIALELEMENTTYPE: "IfcSpatialElementType",
  IFCSPATIALELEMENT: "IfcSpatialElement",
  IFCSIMPLEPROPERTYTEMPLATE: "IfcSimplePropertyTemplate",
  IFCREVOLVEDAREASOLIDTAPERED: "IfcRevolvedAreaSolidTapered",
  IFCREPARAMETRISEDCOMPOSITECURVESEGMENT: "IfcReparametrisedCompositeCurveSegment",
  IFCRELSPACEBOUNDARY2NDLEVEL: "IfcRelSpaceBoundary2ndLevel",
  IFCRELSPACEBOUNDARY1STLEVEL: "IfcRelSpaceBoundary1stLevel",
  IFCRELINTERFERESELEMENTS: "IfcRelInterferesElements",
  IFCRELDEFINESBYTEMPLATE: "IfcRelDefinesByTemplate",
  IFCRELDEFINESBYOBJECT: "IfcRelDefinesByObject",
  IFCRELDECLARES: "IfcRelDeclares",
  IFCRELASSIGNSTOGROUPBYFACTOR: "IfcRelAssignsToGroupByFactor",
  IFCPROPERTYTEMPLATE: "IfcPropertyTemplate",
  IFCPROPERTYSETTEMPLATE: "IfcPropertySetTemplate",
  IFCPROJECTLIBRARY: "IfcProjectLibrary",
  IFCPROCEDURETYPE: "IfcProcedureType",
  IFCPREDEFINEDPROPERTYSET: "IfcPredefinedPropertySet",
  IFCPCURVE: "IfcPCurve",
  IFCLABORRESOURCETYPE: "IfcLaborResourceType",
  IFCINDEXEDPOLYGONALFACEWITHVOIDS: "IfcIndexedPolygonalFaceWithVoids",
  IFCINDEXEDPOLYGONALFACE: "IfcIndexedPolygonalFace",
  IFCGEOGRAPHICELEMENTTYPE: "IfcGeographicElementType",
  IFCFIXEDREFERENCESWEPTAREASOLID: "IfcFixedReferenceSweptAreaSolid",
  IFCEXTRUDEDAREASOLIDTAPERED: "IfcExtrudedAreaSolidTapered",
  IFCEVENTTYPE: "IfcEventType",
  IFCCURVEBOUNDEDSURFACE: "IfcCurveBoundedSurface",
  IFCCREWRESOURCETYPE: "IfcCrewResourceType",
  IFCCONTEXT: "IfcContext",
  IFCCONSTRUCTIONRESOURCETYPE: "IfcConstructionResourceType",
  IFCCARTESIANPOINTLIST3D: "IfcCartesianPointList3D",
  IFCCARTESIANPOINTLIST2D: "IfcCartesianPointList2D",
  IFCCARTESIANPOINTLIST: "IfcCartesianPointList",
  IFCADVANCEDFACE: "IfcAdvancedFace",
  IFCTYPERESOURCE: "IfcTypeResource",
  IFCTYPEPROCESS: "IfcTypeProcess",
  IFCTESSELLATEDITEM: "IfcTessellatedItem",
  IFCSWEPTDISKSOLIDPOLYGONAL: "IfcSweptDiskSolidPolygonal",
  IFCRESOURCETIME: "IfcResourceTime",
  IFCRESOURCECONSTRAINTRELATIONSHIP: "IfcResourceConstraintRelationship",
  IFCRESOURCEAPPROVALRELATIONSHIP: "IfcResourceApprovalRelationship",
  IFCQUANTITYSET: "IfcQuantitySet",
  IFCPROPERTYTEMPLATEDEFINITION: "IfcPropertyTemplateDefinition",
  IFCPREDEFINEDPROPERTIES: "IfcPredefinedProperties",
  IFCMIRROREDPROFILEDEF: "IfcMirroredProfileDef",
  IFCMATERIALRELATIONSHIP: "IfcMaterialRelationship",
  IFCMATERIALPROFILESETUSAGETAPERING: "IfcMaterialProfileSetUsageTapering",
  IFCMATERIALPROFILESETUSAGE: "IfcMaterialProfileSetUsage",
  IFCMATERIALCONSTITUENTSET: "IfcMaterialConstituentSet",
  IFCMATERIALCONSTITUENT: "IfcMaterialConstituent",
  IFCLAGTIME: "IfcLagTime",
  IFCINDEXEDTRIANGLETEXTUREMAP: "IfcIndexedTriangleTextureMap",
  IFCINDEXEDTEXTUREMAP: "IfcIndexedTextureMap",
  IFCINDEXEDCOLOURMAP: "IfcIndexedColourMap",
  IFCEXTERNALREFERENCERELATIONSHIP: "IfcExternalReferenceRelationship",
  IFCEXTENDEDPROPERTIES: "IfcExtendedProperties",
  IFCEVENTTIME: "IfcEventTime",
  IFCCONVERSIONBASEDUNITWITHOFFSET: "IfcConversionBasedUnitWithOffset",
  IFCCOLOURRGBLIST: "IfcColourRgbList",
  IFCWORKTIME: "IfcWorkTime",
  IFCTIMEPERIOD: "IfcTimePeriod",
  IFCTEXTUREVERTEXLIST: "IfcTextureVertexList",
  IFCTASKTIMERECURRING: "IfcTaskTimeRecurring",
  IFCTASKTIME: "IfcTaskTime",
  IFCTABLECOLUMN: "IfcTableColumn",
  IFCSURFACEREINFORCEMENTAREA: "IfcSurfaceReinforcementArea",
  IFCSTRUCTURALLOADORRESULT: "IfcStructuralLoadOrResult",
  IFCSTRUCTURALLOADCONFIGURATION: "IfcStructuralLoadConfiguration",
  IFCSCHEDULINGTIME: "IfcSchedulingTime",
  IFCRESOURCELEVELRELATIONSHIP: "IfcResourceLevelRelationship",
  IFCREFERENCE: "IfcReference",
  IFCRECURRENCEPATTERN: "IfcRecurrencePattern",
  IFCPROPERTYABSTRACTION: "IfcPropertyAbstraction",
  IFCPROJECTEDCRS: "IfcProjectedCrs",
  IFCPRESENTATIONITEM: "IfcPresentationItem",
  IFCMATERIALUSAGEDEFINITION: "IfcMaterialUsageDefinition",
  IFCMATERIALPROFILEWITHOFFSETS: "IfcMaterialProfileWithOffsets",
  IFCMATERIALPROFILESET: "IfcMaterialProfileSet",
  IFCMATERIALPROFILE: "IfcMaterialProfile",
  IFCMATERIALLAYERWITHOFFSETS: "IfcMaterialLayerWithOffsets",
  IFCMATERIALDEFINITION: "IfcMaterialDefinition",
  IFCMAPCONVERSION: "IfcMapConversion",
  IFCEXTERNALINFORMATION: "IfcExternalInformation",
  IFCCOORDINATEREFERENCESYSTEM: "IfcCoordinateReferenceSystem",
  IFCCOORDINATEOPERATION: "IfcCoordinateOperation",
  IFCCONNECTIONVOLUMEGEOMETRY: "IfcConnectionVolumeGeometry",
  IFCREINFORCINGBAR: "IfcReinforcingBar",
  IFCELECTRICDISTRIBUTIONPOINT: "IfcElectricDistributionPoint",
  IFCDISTRIBUTIONCONTROLELEMENT: "IfcDistributionControlElement",
  IFCDISTRIBUTIONCHAMBERELEMENT: "IfcDistributionChamberElement",
  IFCCONTROLLERTYPE: "IfcControllerType",
  IFCCHAMFEREDGEFEATURE: "IfcChamferEdgeFeature",
  IFCBEAM: "IfcBeam",
  IFCALARMTYPE: "IfcAlarmType",
  IFCACTUATORTYPE: "IfcActuatorType",
  IFCWINDOW: "IfcWindow",
  IFCWALLSTANDARDCASE: "IfcWallStandardCase",
  IFCWALL: "IfcWall",
  IFCVIBRATIONISOLATORTYPE: "IfcVibrationIsolatorType",
  IFCTENDONANCHOR: "IfcTendonAnchor",
  IFCTENDON: "IfcTendon",
  IFCSTRUCTURALANALYSISMODEL: "IfcStructuralAnalysisModel",
  IFCSTAIRFLIGHT: "IfcStairFlight",
  IFCSTAIR: "IfcStair",
  IFCSLAB: "IfcSlab",
  IFCSENSORTYPE: "IfcSensorType",
  IFCROUNDEDEDGEFEATURE: "IfcRoundedEdgeFeature",
  IFCROOF: "IfcRoof",
  IFCREINFORCINGMESH: "IfcReinforcingMesh",
  IFCREINFORCINGELEMENT: "IfcReinforcingElement",
  IFCRATIONALBEZIERCURVE: "IfcRationalBezierCurve",
  IFCRAMPFLIGHT: "IfcRampFlight",
  IFCRAMP: "IfcRamp",
  IFCRAILING: "IfcRailing",
  IFCPLATE: "IfcPlate",
  IFCPILE: "IfcPile",
  IFCMEMBER: "IfcMember",
  IFCFOOTING: "IfcFooting",
  IFCFLOWTREATMENTDEVICE: "IfcFlowTreatmentDevice",
  IFCFLOWTERMINAL: "IfcFlowTerminal",
  IFCFLOWSTORAGEDEVICE: "IfcFlowStorageDevice",
  IFCFLOWSEGMENT: "IfcFlowSegment",
  IFCFLOWMOVINGDEVICE: "IfcFlowMovingDevice",
  IFCFLOWINSTRUMENTTYPE: "IfcFlowInstrumentType",
  IFCFLOWFITTING: "IfcFlowFitting",
  IFCFLOWCONTROLLER: "IfcFlowController",
  IFCFIRESUPPRESSIONTERMINALTYPE: "IfcFireSuppressionTerminalType",
  IFCFILTERTYPE: "IfcFilterType",
  IFCFANTYPE: "IfcFanType",
  IFCENERGYCONVERSIONDEVICE: "IfcEnergyConversionDevice",
  IFCELECTRICALELEMENT: "IfcElectricalElement",
  IFCELECTRICALCIRCUIT: "IfcElectricalCircuit",
  IFCELECTRICTIMECONTROLTYPE: "IfcElectricTimeControlType",
  IFCELECTRICMOTORTYPE: "IfcElectricMotorType",
  IFCELECTRICHEATERTYPE: "IfcElectricHeaterType",
  IFCELECTRICGENERATORTYPE: "IfcElectricGeneratorType",
  IFCELECTRICFLOWSTORAGEDEVICETYPE: "IfcElectricFlowStorageDeviceType",
  IFCELECTRICAPPLIANCETYPE: "IfcElectricApplianceType",
  IFCEDGEFEATURE: "IfcEdgeFeature",
  IFCDUCTSILENCERTYPE: "IfcDuctSilencerType",
  IFCDUCTSEGMENTTYPE: "IfcDuctSegmentType",
  IFCDUCTFITTINGTYPE: "IfcDuctFittingType",
  IFCDOOR: "IfcDoor",
  IFCDISTRIBUTIONPORT: "IfcDistributionPort",
  IFCDISTRIBUTIONFLOWELEMENT: "IfcDistributionFlowElement",
  IFCDISTRIBUTIONELEMENT: "IfcDistributionElement",
  IFCDISTRIBUTIONCONTROLELEMENTTYPE: "IfcDistributionControlElementType",
  IFCDISTRIBUTIONCHAMBERELEMENTTYPE: "IfcDistributionChamberElementType",
  IFCDISCRETEACCESSORYTYPE: "IfcDiscreteAccessoryType",
  IFCDISCRETEACCESSORY: "IfcDiscreteAccessory",
  IFCDIAMETERDIMENSION: "IfcDiameterDimension",
  IFCDAMPERTYPE: "IfcDamperType",
  IFCCURTAINWALL: "IfcCurtainWall",
  IFCCOVERING: "IfcCovering",
  IFCCOOLINGTOWERTYPE: "IfcCoolingTowerType",
  IFCCOOLEDBEAMTYPE: "IfcCooledBeamType",
  IFCCONSTRUCTIONPRODUCTRESOURCE: "IfcConstructionProductResource",
  IFCCONSTRUCTIONMATERIALRESOURCE: "IfcConstructionMaterialResource",
  IFCCONSTRUCTIONEQUIPMENTRESOURCE: "IfcConstructionEquipmentResource",
  IFCCONDITIONCRITERION: "IfcConditionCriterion",
  IFCCONDITION: "IfcCondition",
  IFCCONDENSERTYPE: "IfcCondenserType",
  IFCCOMPRESSORTYPE: "IfcCompressorType",
  IFCCOLUMN: "IfcColumn",
  IFCCOILTYPE: "IfcCoilType",
  IFCCIRCLE: "IfcCircle",
  IFCCHILLERTYPE: "IfcChillerType",
  IFCCABLESEGMENTTYPE: "IfcCableSegmentType",
  IFCCABLECARRIERSEGMENTTYPE: "IfcCableCarrierSegmentType",
  IFCCABLECARRIERFITTINGTYPE: "IfcCableCarrierFittingType",
  IFCBUILDINGELEMENTPROXYTYPE: "IfcBuildingElementProxyType",
  IFCBUILDINGELEMENTPROXY: "IfcBuildingElementProxy",
  IFCBUILDINGELEMENTPART: "IfcBuildingElementPart",
  IFCBUILDINGELEMENTCOMPONENT: "IfcBuildingElementComponent",
  IFCBUILDINGELEMENT: "IfcBuildingElement",
  IFCBOILERTYPE: "IfcBoilerType",
  IFCBEZIERCURVE: "IfcBezierCurve",
  IFCBEAMTYPE: "IfcBeamType",
  IFCBSPLINECURVE: "IfcBSplineCurve",
  IFCASSET: "IfcAsset",
  IFCANGULARDIMENSION: "IfcAngularDimension",
  IFCAIRTOAIRHEATRECOVERYTYPE: "IfcAirToAirHeatRecoveryType",
  IFCAIRTERMINALTYPE: "IfcAirTerminalType",
  IFCAIRTERMINALBOXTYPE: "IfcAirTerminalBoxType",
  IFCACTIONREQUEST: "IfcActionRequest",
  IFC2DCOMPOSITECURVE: "Ifc2DCompositeCurve",
  IFCZONE: "IfcZone",
  IFCWORKSCHEDULE: "IfcWorkSchedule",
  IFCWORKPLAN: "IfcWorkPlan",
  IFCWORKCONTROL: "IfcWorkControl",
  IFCWASTETERMINALTYPE: "IfcWasteTerminalType",
  IFCWALLTYPE: "IfcWallType",
  IFCVIRTUALELEMENT: "IfcVirtualElement",
  IFCVALVETYPE: "IfcValveType",
  IFCUNITARYEQUIPMENTTYPE: "IfcUnitaryEquipmentType",
  IFCTUBEBUNDLETYPE: "IfcTubeBundleType",
  IFCTRIMMEDCURVE: "IfcTrimmedCurve",
  IFCTRANSPORTELEMENT: "IfcTransportElement",
  IFCTRANSFORMERTYPE: "IfcTransformerType",
  IFCTIMESERIESSCHEDULE: "IfcTimeSeriesSchedule",
  IFCTANKTYPE: "IfcTankType",
  IFCSYSTEM: "IfcSystem",
  IFCSWITCHINGDEVICETYPE: "IfcSwitchingDeviceType",
  IFCSUBCONTRACTRESOURCE: "IfcSubContractResource",
  IFCSTRUCTURALSURFACECONNECTION: "IfcStructuralSurfaceConnection",
  IFCSTRUCTURALRESULTGROUP: "IfcStructuralResultGroup",
  IFCSTRUCTURALPOINTREACTION: "IfcStructuralPointReaction",
  IFCSTRUCTURALPOINTCONNECTION: "IfcStructuralPointConnection",
  IFCSTRUCTURALPOINTACTION: "IfcStructuralPointAction",
  IFCSTRUCTURALPLANARACTIONVARYING: "IfcStructuralPlanarActionVarying",
  IFCSTRUCTURALPLANARACTION: "IfcStructuralPlanarAction",
  IFCSTRUCTURALLOADGROUP: "IfcStructuralLoadGroup",
  IFCSTRUCTURALLINEARACTIONVARYING: "IfcStructuralLinearActionVarying",
  IFCSTRUCTURALLINEARACTION: "IfcStructuralLinearAction",
  IFCSTRUCTURALCURVEMEMBERVARYING: "IfcStructuralCurveMemberVarying",
  IFCSTRUCTURALCURVEMEMBER: "IfcStructuralCurveMember",
  IFCSTRUCTURALCURVECONNECTION: "IfcStructuralCurveConnection",
  IFCSTRUCTURALCONNECTION: "IfcStructuralConnection",
  IFCSTRUCTURALACTION: "IfcStructuralAction",
  IFCSTAIRFLIGHTTYPE: "IfcStairFlightType",
  IFCSTACKTERMINALTYPE: "IfcStackTerminalType",
  IFCSPACETYPE: "IfcSpaceType",
  IFCSPACEPROGRAM: "IfcSpaceProgram",
  IFCSPACEHEATERTYPE: "IfcSpaceHeaterType",
  IFCSPACE: "IfcSpace",
  IFCSLABTYPE: "IfcSlabType",
  IFCSITE: "IfcSite",
  IFCSERVICELIFE: "IfcServiceLife",
  IFCSCHEDULETIMECONTROL: "IfcScheduleTimeControl",
  IFCSANITARYTERMINALTYPE: "IfcSanitaryTerminalType",
  IFCRELASSIGNSTASKS: "IfcRelAssignsTasks",
  IFCRELAGGREGATES: "IfcRelAggregates",
  IFCRAMPFLIGHTTYPE: "IfcRampFlightType",
  IFCRAILINGTYPE: "IfcRailingType",
  IFCRADIUSDIMENSION: "IfcRadiusDimension",
  IFCPUMPTYPE: "IfcPumpType",
  IFCPROTECTIVEDEVICETYPE: "IfcProtectiveDeviceType",
  IFCPROJECTIONELEMENT: "IfcProjectionElement",
  IFCPROJECTORDERRECORD: "IfcProjectOrderRecord",
  IFCPROJECTORDER: "IfcProjectOrder",
  IFCPROCEDURE: "IfcProcedure",
  IFCPORT: "IfcPort",
  IFCPOLYLINE: "IfcPolyline",
  IFCPLATETYPE: "IfcPlateType",
  IFCPIPESEGMENTTYPE: "IfcPipeSegmentType",
  IFCPIPEFITTINGTYPE: "IfcPipeFittingType",
  IFCPERMIT: "IfcPermit",
  IFCPERFORMANCEHISTORY: "IfcPerformanceHistory",
  IFCOUTLETTYPE: "IfcOutletType",
  IFCORDERACTION: "IfcOrderAction",
  IFCOPENINGELEMENT: "IfcOpeningElement",
  IFCOCCUPANT: "IfcOccupant",
  IFCMOVE: "IfcMove",
  IFCMOTORCONNECTIONTYPE: "IfcMotorConnectionType",
  IFCMEMBERTYPE: "IfcMemberType",
  IFCMECHANICALFASTENERTYPE: "IfcMechanicalFastenerType",
  IFCMECHANICALFASTENER: "IfcMechanicalFastener",
  IFCLINEARDIMENSION: "IfcLinearDimension",
  IFCLIGHTFIXTURETYPE: "IfcLightFixtureType",
  IFCLAMPTYPE: "IfcLampType",
  IFCLABORRESOURCE: "IfcLaborResource",
  IFCJUNCTIONBOXTYPE: "IfcJunctionBoxType",
  IFCINVENTORY: "IfcInventory",
  IFCHUMIDIFIERTYPE: "IfcHumidifierType",
  IFCHEATEXCHANGERTYPE: "IfcHeatExchangerType",
  IFCGROUP: "IfcGroup",
  IFCGRID: "IfcGrid",
  IFCGASTERMINALTYPE: "IfcGasTerminalType",
  IFCFURNITURESTANDARD: "IfcFurnitureStandard",
  IFCFURNISHINGELEMENT: "IfcFurnishingElement",
  IFCFLOWTREATMENTDEVICETYPE: "IfcFlowTreatmentDeviceType",
  IFCFLOWTERMINALTYPE: "IfcFlowTerminalType",
  IFCFLOWSTORAGEDEVICETYPE: "IfcFlowStorageDeviceType",
  IFCFLOWSEGMENTTYPE: "IfcFlowSegmentType",
  IFCFLOWMOVINGDEVICETYPE: "IfcFlowMovingDeviceType",
  IFCFLOWMETERTYPE: "IfcFlowMeterType",
  IFCFLOWFITTINGTYPE: "IfcFlowFittingType",
  IFCFLOWCONTROLLERTYPE: "IfcFlowControllerType",
  IFCFEATUREELEMENTSUBTRACTION: "IfcFeatureElementSubtraction",
  IFCFEATUREELEMENTADDITION: "IfcFeatureElementAddition",
  IFCFEATUREELEMENT: "IfcFeatureElement",
  IFCFASTENERTYPE: "IfcFastenerType",
  IFCFASTENER: "IfcFastener",
  IFCFACETEDBREPWITHVOIDS: "IfcFacetedBrepWithVoids",
  IFCFACETEDBREP: "IfcFacetedBrep",
  IFCEVAPORATORTYPE: "IfcEvaporatorType",
  IFCEVAPORATIVECOOLERTYPE: "IfcEvaporativeCoolerType",
  IFCEQUIPMENTSTANDARD: "IfcEquipmentStandard",
  IFCEQUIPMENTELEMENT: "IfcEquipmentElement",
  IFCENERGYCONVERSIONDEVICETYPE: "IfcEnergyConversionDeviceType",
  IFCELLIPSE: "IfcEllipse",
  IFCELEMENTCOMPONENTTYPE: "IfcElementComponentType",
  IFCELEMENTCOMPONENT: "IfcElementComponent",
  IFCELEMENTASSEMBLY: "IfcElementAssembly",
  IFCELEMENT: "IfcElement",
  IFCELECTRICALBASEPROPERTIES: "IfcElectricalBaseProperties",
  IFCDISTRIBUTIONFLOWELEMENTTYPE: "IfcDistributionFlowElementType",
  IFCDISTRIBUTIONELEMENTTYPE: "IfcDistributionElementType",
  IFCDIMENSIONCURVEDIRECTEDCALLOUT: "IfcDimensionCurveDirectedCallout",
  IFCCURTAINWALLTYPE: "IfcCurtainWallType",
  IFCCREWRESOURCE: "IfcCrewResource",
  IFCCOVERINGTYPE: "IfcCoveringType",
  IFCCOSTSCHEDULE: "IfcCostSchedule",
  IFCCOSTITEM: "IfcCostItem",
  IFCCONTROL: "IfcControl",
  IFCCONSTRUCTIONRESOURCE: "IfcConstructionResource",
  IFCCONIC: "IfcConic",
  IFCCOMPOSITECURVE: "IfcCompositeCurve",
  IFCCOLUMNTYPE: "IfcColumnType",
  IFCCIRCLEHOLLOWPROFILEDEF: "IfcCircleHollowProfileDef",
  IFCBUILDINGSTOREY: "IfcBuildingStorey",
  IFCBUILDINGELEMENTTYPE: "IfcBuildingElementType",
  IFCBUILDING: "IfcBuilding",
  IFCBOUNDEDCURVE: "IfcBoundedCurve",
  IFCBOOLEANCLIPPINGRESULT: "IfcBooleanClippingResult",
  IFCBLOCK: "IfcBlock",
  IFCASYMMETRICISHAPEPROFILEDEF: "IfcAsymmetricIShapeProfileDef",
  IFCANNOTATION: "IfcAnnotation",
  IFCACTOR: "IfcActor",
  IFCTRANSPORTELEMENTTYPE: "IfcTransportElementType",
  IFCTASK: "IfcTask",
  IFCSYSTEMFURNITUREELEMENTTYPE: "IfcSystemFurnitureElementType",
  IFCSURFACEOFREVOLUTION: "IfcSurfaceOfRevolution",
  IFCSURFACEOFLINEAREXTRUSION: "IfcSurfaceOfLinearExtrusion",
  IFCSURFACECURVESWEPTAREASOLID: "IfcSurfaceCurveSweptAreaSolid",
  IFCSTRUCTUREDDIMENSIONCALLOUT: "IfcStructuredDimensionCallout",
  IFCSTRUCTURALSURFACEMEMBERVARYING: "IfcStructuralSurfaceMemberVarying",
  IFCSTRUCTURALSURFACEMEMBER: "IfcStructuralSurfaceMember",
  IFCSTRUCTURALREACTION: "IfcStructuralReaction",
  IFCSTRUCTURALMEMBER: "IfcStructuralMember",
  IFCSTRUCTURALITEM: "IfcStructuralItem",
  IFCSTRUCTURALACTIVITY: "IfcStructuralActivity",
  IFCSPHERE: "IfcSphere",
  IFCSPATIALSTRUCTUREELEMENTTYPE: "IfcSpatialStructureElementType",
  IFCSPATIALSTRUCTUREELEMENT: "IfcSpatialStructureElement",
  IFCRIGHTCIRCULARCYLINDER: "IfcRightCircularCylinder",
  IFCRIGHTCIRCULARCONE: "IfcRightCircularCone",
  IFCREVOLVEDAREASOLID: "IfcRevolvedAreaSolid",
  IFCRESOURCE: "IfcResource",
  IFCRELVOIDSELEMENT: "IfcRelVoidsElement",
  IFCRELSPACEBOUNDARY: "IfcRelSpaceBoundary",
  IFCRELSERVICESBUILDINGS: "IfcRelServicesBuildings",
  IFCRELSEQUENCE: "IfcRelSequence",
  IFCRELSCHEDULESCOSTITEMS: "IfcRelSchedulesCostItems",
  IFCRELREFERENCEDINSPATIALSTRUCTURE: "IfcRelReferencedInSpatialStructure",
  IFCRELPROJECTSELEMENT: "IfcRelProjectsElement",
  IFCRELOVERRIDESPROPERTIES: "IfcRelOverridesProperties",
  IFCRELOCCUPIESSPACES: "IfcRelOccupiesSpaces",
  IFCRELNESTS: "IfcRelNests",
  IFCRELINTERACTIONREQUIREMENTS: "IfcRelInteractionRequirements",
  IFCRELFLOWCONTROLELEMENTS: "IfcRelFlowControlElements",
  IFCRELFILLSELEMENT: "IfcRelFillsElement",
  IFCRELDEFINESBYTYPE: "IfcRelDefinesByType",
  IFCRELDEFINESBYPROPERTIES: "IfcRelDefinesByProperties",
  IFCRELDEFINES: "IfcRelDefines",
  IFCRELDECOMPOSES: "IfcRelDecomposes",
  IFCRELCOVERSSPACES: "IfcRelCoversSpaces",
  IFCRELCOVERSBLDGELEMENTS: "IfcRelCoversBldgElements",
  IFCRELCONTAINEDINSPATIALSTRUCTURE: "IfcRelContainedInSpatialStructure",
  IFCRELCONNECTSWITHREALIZINGELEMENTS: "IfcRelConnectsWithRealizingElements",
  IFCRELCONNECTSWITHECCENTRICITY: "IfcRelConnectsWithEccentricity",
  IFCRELCONNECTSSTRUCTURALMEMBER: "IfcRelConnectsStructuralMember",
  IFCRELCONNECTSSTRUCTURALELEMENT: "IfcRelConnectsStructuralElement",
  IFCRELCONNECTSSTRUCTURALACTIVITY: "IfcRelConnectsStructuralActivity",
  IFCRELCONNECTSPORTS: "IfcRelConnectsPorts",
  IFCRELCONNECTSPORTTOELEMENT: "IfcRelConnectsPortToElement",
  IFCRELCONNECTSPATHELEMENTS: "IfcRelConnectsPathElements",
  IFCRELCONNECTSELEMENTS: "IfcRelConnectsElements",
  IFCRELCONNECTS: "IfcRelConnects",
  IFCRELASSOCIATESPROFILEPROPERTIES: "IfcRelAssociatesProfileProperties",
  IFCRELASSOCIATESMATERIAL: "IfcRelAssociatesMaterial",
  IFCRELASSOCIATESLIBRARY: "IfcRelAssociatesLibrary",
  IFCRELASSOCIATESDOCUMENT: "IfcRelAssociatesDocument",
  IFCRELASSOCIATESCONSTRAINT: "IfcRelAssociatesConstraint",
  IFCRELASSOCIATESCLASSIFICATION: "IfcRelAssociatesClassification",
  IFCRELASSOCIATESAPPROVAL: "IfcRelAssociatesApproval",
  IFCRELASSOCIATESAPPLIEDVALUE: "IfcRelAssociatesAppliedValue",
  IFCRELASSOCIATES: "IfcRelAssociates",
  IFCRELASSIGNSTORESOURCE: "IfcRelAssignsToResource",
  IFCRELASSIGNSTOPROJECTORDER: "IfcRelAssignsToProjectOrder",
  IFCRELASSIGNSTOPRODUCT: "IfcRelAssignsToProduct",
  IFCRELASSIGNSTOPROCESS: "IfcRelAssignsToProcess",
  IFCRELASSIGNSTOGROUP: "IfcRelAssignsToGroup",
  IFCRELASSIGNSTOCONTROL: "IfcRelAssignsToControl",
  IFCRELASSIGNSTOACTOR: "IfcRelAssignsToActor",
  IFCRELASSIGNS: "IfcRelAssigns",
  IFCRECTANGULARTRIMMEDSURFACE: "IfcRectangularTrimmedSurface",
  IFCRECTANGULARPYRAMID: "IfcRectangularPyramid",
  IFCRECTANGLEHOLLOWPROFILEDEF: "IfcRectangleHollowProfileDef",
  IFCPROXY: "IfcProxy",
  IFCPROPERTYSET: "IfcPropertySet",
  IFCPROJECTIONCURVE: "IfcProjectionCurve",
  IFCPROJECT: "IfcProject",
  IFCPRODUCT: "IfcProduct",
  IFCPROCESS: "IfcProcess",
  IFCPLANE: "IfcPlane",
  IFCPLANARBOX: "IfcPlanarBox",
  IFCPERMEABLECOVERINGPROPERTIES: "IfcPermeableCoveringProperties",
  IFCOFFSETCURVE3D: "IfcOffsetCurve3D",
  IFCOFFSETCURVE2D: "IfcOffsetCurve2D",
  IFCOBJECT: "IfcObject",
  IFCMANIFOLDSOLIDBREP: "IfcManifoldSolidBrep",
  IFCLINE: "IfcLine",
  IFCLSHAPEPROFILEDEF: "IfcLShapeProfileDef",
  IFCISHAPEPROFILEDEF: "IfcIShapeProfileDef",
  IFCGEOMETRICCURVESET: "IfcGeometricCurveSet",
  IFCFURNITURETYPE: "IfcFurnitureType",
  IFCFURNISHINGELEMENTTYPE: "IfcFurnishingElementType",
  IFCFLUIDFLOWPROPERTIES: "IfcFluidFlowProperties",
  IFCFILLAREASTYLETILES: "IfcFillAreaStyleTiles",
  IFCFILLAREASTYLETILESYMBOLWITHSTYLE: "IfcFillAreaStyleTileSymbolWithStyle",
  IFCFILLAREASTYLEHATCHING: "IfcFillAreaStyleHatching",
  IFCFACEBASEDSURFACEMODEL: "IfcFaceBasedSurfaceModel",
  IFCEXTRUDEDAREASOLID: "IfcExtrudedAreaSolid",
  IFCENERGYPROPERTIES: "IfcEnergyProperties",
  IFCELLIPSEPROFILEDEF: "IfcEllipseProfileDef",
  IFCELEMENTARYSURFACE: "IfcElementarySurface",
  IFCELEMENTTYPE: "IfcElementType",
  IFCELEMENTQUANTITY: "IfcElementQuantity",
  IFCEDGELOOP: "IfcEdgeLoop",
  IFCDRAUGHTINGPREDEFINEDCURVEFONT: "IfcDraughtingPredefinedCurveFont",
  IFCDRAUGHTINGPREDEFINEDCOLOUR: "IfcDraughtingPredefinedColour",
  IFCDRAUGHTINGCALLOUT: "IfcDraughtingCallout",
  IFCDOORSTYLE: "IfcDoorStyle",
  IFCDOORPANELPROPERTIES: "IfcDoorPanelProperties",
  IFCDOORLININGPROPERTIES: "IfcDoorLiningProperties",
  IFCDIRECTION: "IfcDirection",
  IFCDIMENSIONCURVETERMINATOR: "IfcDimensionCurveTerminator",
  IFCDIMENSIONCURVE: "IfcDimensionCurve",
  IFCDEFINEDSYMBOL: "IfcDefinedSymbol",
  IFCCURVEBOUNDEDPLANE: "IfcCurveBoundedPlane",
  IFCCURVE: "IfcCurve",
  IFCCSGSOLID: "IfcCsgSolid",
  IFCCSGPRIMITIVE3D: "IfcCsgPrimitive3D",
  IFCCRANERAILFSHAPEPROFILEDEF: "IfcCraneRailFShapeProfileDef",
  IFCCRANERAILASHAPEPROFILEDEF: "IfcCraneRailAShapeProfileDef",
  IFCCOMPOSITECURVESEGMENT: "IfcCompositeCurveSegment",
  IFCCLOSEDSHELL: "IfcClosedShell",
  IFCCIRCLEPROFILEDEF: "IfcCircleProfileDef",
  IFCCARTESIANTRANSFORMATIONOPERATOR3DNONUNIFORM: "IfcCartesianTransformationOperator3DNonUniform",
  IFCCARTESIANTRANSFORMATIONOPERATOR3D: "IfcCartesianTransformationOperator3D",
  IFCCARTESIANTRANSFORMATIONOPERATOR2DNONUNIFORM: "IfcCartesianTransformationOperator2DNonUniform",
  IFCCARTESIANTRANSFORMATIONOPERATOR2D: "IfcCartesianTransformationOperator2D",
  IFCCARTESIANTRANSFORMATIONOPERATOR: "IfcCartesianTransformationOperator",
  IFCCARTESIANPOINT: "IfcCartesianPoint",
  IFCCSHAPEPROFILEDEF: "IfcCShapeProfileDef",
  IFCBOXEDHALFSPACE: "IfcBoxedHalfSpace",
  IFCBOUNDINGBOX: "IfcBoundingBox",
  IFCBOUNDEDSURFACE: "IfcBoundedSurface",
  IFCBOOLEANRESULT: "IfcBooleanResult",
  IFCAXIS2PLACEMENT3D: "IfcAxis2Placement3D",
  IFCAXIS2PLACEMENT2D: "IfcAxis2Placement2D",
  IFCAXIS1PLACEMENT: "IfcAxis1Placement",
  IFCANNOTATIONSURFACE: "IfcAnnotationSurface",
  IFCANNOTATIONFILLAREAOCCURRENCE: "IfcAnnotationFillAreaOccurrence",
  IFCANNOTATIONFILLAREA: "IfcAnnotationFillArea",
  IFCANNOTATIONCURVEOCCURRENCE: "IfcAnnotationCurveOccurrence",
  IFCZSHAPEPROFILEDEF: "IfcZShapeProfileDef",
  IFCWINDOWSTYLE: "IfcWindowStyle",
  IFCWINDOWPANELPROPERTIES: "IfcWindowPanelProperties",
  IFCWINDOWLININGPROPERTIES: "IfcWindowLiningProperties",
  IFCVERTEXLOOP: "IfcVertexLoop",
  IFCVECTOR: "IfcVector",
  IFCUSHAPEPROFILEDEF: "IfcUShapeProfileDef",
  IFCTYPEPRODUCT: "IfcTypeProduct",
  IFCTYPEOBJECT: "IfcTypeObject",
  IFCTWODIRECTIONREPEATFACTOR: "IfcTwoDirectionRepeatFactor",
  IFCTRAPEZIUMPROFILEDEF: "IfcTrapeziumProfileDef",
  IFCTEXTLITERALWITHEXTENT: "IfcTextLiteralWithExtent",
  IFCTEXTLITERAL: "IfcTextLiteral",
  IFCTERMINATORSYMBOL: "IfcTerminatorSymbol",
  IFCTSHAPEPROFILEDEF: "IfcTShapeProfileDef",
  IFCSWEPTSURFACE: "IfcSweptSurface",
  IFCSWEPTDISKSOLID: "IfcSweptDiskSolid",
  IFCSWEPTAREASOLID: "IfcSweptAreaSolid",
  IFCSURFACESTYLERENDERING: "IfcSurfaceStyleRendering",
  IFCSURFACE: "IfcSurface",
  IFCSUBEDGE: "IfcSubedge",
  IFCSTRUCTURALSTEELPROFILEPROPERTIES: "IfcStructuralSteelProfileProperties",
  IFCSTRUCTURALPROFILEPROPERTIES: "IfcStructuralProfileProperties",
  IFCSTRUCTURALLOADSINGLEFORCEWARPING: "IfcStructuralLoadSingleForceWarping",
  IFCSTRUCTURALLOADSINGLEFORCE: "IfcStructuralLoadSingleForce",
  IFCSTRUCTURALLOADSINGLEDISPLACEMENTDISTORTION: "IfcStructuralLoadSingleDisplacementDistortion",
  IFCSTRUCTURALLOADSINGLEDISPLACEMENT: "IfcStructuralLoadSingleDisplacement",
  IFCSTRUCTURALLOADPLANARFORCE: "IfcStructuralLoadPlanarForce",
  IFCSTRUCTURALLOADLINEARFORCE: "IfcStructuralLoadLinearForce",
  IFCSPACETHERMALLOADPROPERTIES: "IfcSpaceThermalLoadProperties",
  IFCSOUNDVALUE: "IfcSoundValue",
  IFCSOUNDPROPERTIES: "IfcSoundProperties",
  IFCSOLIDMODEL: "IfcSolidModel",
  IFCSLIPPAGECONNECTIONCONDITION: "IfcSlippageConnectionCondition",
  IFCSHELLBASEDSURFACEMODEL: "IfcShellBasedSurfaceModel",
  IFCSERVICELIFEFACTOR: "IfcServiceLifeFactor",
  IFCSECTIONEDSPINE: "IfcSectionedSpine",
  IFCROUNDEDRECTANGLEPROFILEDEF: "IfcRoundedRectangleProfileDef",
  IFCRELATIONSHIP: "IfcRelationship",
  IFCREINFORCEMENTDEFINITIONPROPERTIES: "IfcReinforcementDefinitionProperties",
  IFCREGULARTIMESERIES: "IfcRegularTimeSeries",
  IFCRECTANGLEPROFILEDEF: "IfcRectangleProfileDef",
  IFCPROPERTYTABLEVALUE: "IfcPropertyTableValue",
  IFCPROPERTYSINGLEVALUE: "IfcPropertySingleValue",
  IFCPROPERTYSETDEFINITION: "IfcPropertySetDefinition",
  IFCPROPERTYREFERENCEVALUE: "IfcPropertyReferenceValue",
  IFCPROPERTYLISTVALUE: "IfcPropertyListValue",
  IFCPROPERTYENUMERATEDVALUE: "IfcPropertyEnumeratedValue",
  IFCPROPERTYDEFINITION: "IfcPropertyDefinition",
  IFCPROPERTYBOUNDEDVALUE: "IfcPropertyBoundedValue",
  IFCPRODUCTDEFINITIONSHAPE: "IfcProductDefinitionShape",
  IFCPREDEFINEDPOINTMARKERSYMBOL: "IfcPredefinedPointMarkerSymbol",
  IFCPREDEFINEDDIMENSIONSYMBOL: "IfcPredefinedDimensionSymbol",
  IFCPREDEFINEDCURVEFONT: "IfcPredefinedCurveFont",
  IFCPREDEFINEDCOLOUR: "IfcPredefinedColour",
  IFCPOLYGONALBOUNDEDHALFSPACE: "IfcPolygonalBoundedHalfSpace",
  IFCPOLYLOOP: "IfcPolyLoop",
  IFCPOINTONSURFACE: "IfcPointOnSurface",
  IFCPOINTONCURVE: "IfcPointOnCurve",
  IFCPOINT: "IfcPoint",
  IFCPLANAREXTENT: "IfcPlanarExtent",
  IFCPLACEMENT: "IfcPlacement",
  IFCPIXELTEXTURE: "IfcPixelTexture",
  IFCPHYSICALCOMPLEXQUANTITY: "IfcPhysicalComplexQuantity",
  IFCPATH: "IfcPath",
  IFCPARAMETERIZEDPROFILEDEF: "IfcParameterizedProfileDef",
  IFCORIENTEDEDGE: "IfcOrientedEdge",
  IFCOPENSHELL: "IfcOpenShell",
  IFCONEDIRECTIONREPEATFACTOR: "IfcOneDirectionRepeatFactor",
  IFCOBJECTDEFINITION: "IfcObjectDefinition",
  IFCMECHANICALCONCRETEMATERIALPROPERTIES: "IfcMechanicalConcreteMaterialProperties",
  IFCMATERIALDEFINITIONREPRESENTATION: "IfcMaterialDefinitionRepresentation",
  IFCMAPPEDITEM: "IfcMappedItem",
  IFCLOOP: "IfcLoop",
  IFCLOCALPLACEMENT: "IfcLocalPlacement",
  IFCLIGHTSOURCESPOT: "IfcLightSourceSpot",
  IFCLIGHTSOURCEPOSITIONAL: "IfcLightSourcePositional",
  IFCLIGHTSOURCEGONIOMETRIC: "IfcLightSourceGoniometric",
  IFCLIGHTSOURCEDIRECTIONAL: "IfcLightSourceDirectional",
  IFCLIGHTSOURCEAMBIENT: "IfcLightSourceAmbient",
  IFCLIGHTSOURCE: "IfcLightSource",
  IFCIRREGULARTIMESERIES: "IfcIrregularTimeSeries",
  IFCIMAGETEXTURE: "IfcImageTexture",
  IFCHYGROSCOPICMATERIALPROPERTIES: "IfcHygroscopicMaterialProperties",
  IFCHALFSPACESOLID: "IfcHalfSpaceSolid",
  IFCGRIDPLACEMENT: "IfcGridPlacement",
  IFCGEOMETRICSET: "IfcGeometricSet",
  IFCGEOMETRICREPRESENTATIONSUBCONTEXT: "IfcGeometricRepresentationSubContext",
  IFCGEOMETRICREPRESENTATIONITEM: "IfcGeometricRepresentationItem",
  IFCGEOMETRICREPRESENTATIONCONTEXT: "IfcGeometricRepresentationContext",
  IFCGENERALPROFILEPROPERTIES: "IfcGeneralProfileProperties",
  IFCGENERALMATERIALPROPERTIES: "IfcGeneralMaterialProperties",
  IFCFUELPROPERTIES: "IfcFuelProperties",
  IFCFILLAREASTYLE: "IfcFillAreaStyle",
  IFCFAILURECONNECTIONCONDITION: "IfcFailureConnectionCondition",
  IFCFACESURFACE: "IfcFaceSurface",
  IFCFACEOUTERBOUND: "IfcFaceOuterBound",
  IFCFACEBOUND: "IfcFaceBound",
  IFCFACE: "IfcFace",
  IFCEXTENDEDMATERIALPROPERTIES: "IfcExtendedMaterialProperties",
  IFCEDGECURVE: "IfcEdgeCurve",
  IFCEDGE: "IfcEdge",
  IFCDRAUGHTINGPREDEFINEDTEXTFONT: "IfcDraughtingPredefinedTextFont",
  IFCDOCUMENTREFERENCE: "IfcDocumentReference",
  IFCDIMENSIONPAIR: "IfcDimensionPair",
  IFCDIMENSIONCALLOUTRELATIONSHIP: "IfcDimensionCalloutRelationship",
  IFCDERIVEDPROFILEDEF: "IfcDerivedProfileDef",
  IFCCURVESTYLE: "IfcCurveStyle",
  IFCCONVERSIONBASEDUNIT: "IfcConversionBasedUnit",
  IFCCONTEXTDEPENDENTUNIT: "IfcContextDependentUnit",
  IFCCONNECTIONPOINTECCENTRICITY: "IfcConnectionPointEccentricity",
  IFCCONNECTIONCURVEGEOMETRY: "IfcConnectionCurveGeometry",
  IFCCONNECTEDFACESET: "IfcConnectedFaceSet",
  IFCCOMPOSITEPROFILEDEF: "IfcCompositeProfileDef",
  IFCCOMPLEXPROPERTY: "IfcComplexProperty",
  IFCCOLOURRGB: "IfcColourRgb",
  IFCCLASSIFICATIONREFERENCE: "IfcClassificationReference",
  IFCCENTERLINEPROFILEDEF: "IfcCenterLineProfileDef",
  IFCBLOBTEXTURE: "IfcBlobTexture",
  IFCARBITRARYPROFILEDEFWITHVOIDS: "IfcArbitraryProfileDefWithVoids",
  IFCARBITRARYOPENPROFILEDEF: "IfcArbitraryOpenProfileDef",
  IFCARBITRARYCLOSEDPROFILEDEF: "IfcArbitraryClosedProfileDef",
  IFCANNOTATIONTEXTOCCURRENCE: "IfcAnnotationTextOccurrence",
  IFCANNOTATIONSYMBOLOCCURRENCE: "IfcAnnotationSymbolOccurrence",
  IFCANNOTATIONSURFACEOCCURRENCE: "IfcAnnotationSurfaceOccurrence",
  IFCANNOTATIONOCCURRENCE: "IfcAnnotationOccurrence",
  IFCWATERPROPERTIES: "IfcWaterProperties",
  IFCVIRTUALGRIDINTERSECTION: "IfcVirtualGridIntersection",
  IFCVERTEXPOINT: "IfcVertexPoint",
  IFCVERTEXBASEDTEXTUREMAP: "IfcVertexBasedTextureMap",
  IFCVERTEX: "IfcVertex",
  IFCUNITASSIGNMENT: "IfcUnitAssignment",
  IFCTOPOLOGYREPRESENTATION: "IfcTopologyRepresentation",
  IFCTOPOLOGICALREPRESENTATIONITEM: "IfcTopologicalRepresentationItem",
  IFCTIMESERIESVALUE: "IfcTimeSeriesValue",
  IFCTIMESERIESREFERENCERELATIONSHIP: "IfcTimeSeriesReferenceRelationship",
  IFCTIMESERIES: "IfcTimeSeries",
  IFCTHERMALMATERIALPROPERTIES: "IfcThermalMaterialProperties",
  IFCTEXTUREVERTEX: "IfcTextureVertex",
  IFCTEXTUREMAP: "IfcTextureMap",
  IFCTEXTURECOORDINATEGENERATOR: "IfcTextureCoordinateGenerator",
  IFCTEXTURECOORDINATE: "IfcTextureCoordinate",
  IFCTEXTSTYLEWITHBOXCHARACTERISTICS: "IfcTextStyleWithBoxCharacteristics",
  IFCTEXTSTYLETEXTMODEL: "IfcTextStyleTextModel",
  IFCTEXTSTYLEFORDEFINEDFONT: "IfcTextStyleForDefinedFont",
  IFCTEXTSTYLEFONTMODEL: "IfcTextStyleFontModel",
  IFCTEXTSTYLE: "IfcTextStyle",
  IFCTELECOMADDRESS: "IfcTelecomAddress",
  IFCTABLEROW: "IfcTableRow",
  IFCTABLE: "IfcTable",
  IFCSYMBOLSTYLE: "IfcSymbolStyle",
  IFCSURFACETEXTURE: "IfcSurfaceTexture",
  IFCSURFACESTYLEWITHTEXTURES: "IfcSurfaceStyleWithTextures",
  IFCSURFACESTYLESHADING: "IfcSurfaceStyleShading",
  IFCSURFACESTYLEREFRACTION: "IfcSurfaceStyleRefraction",
  IFCSURFACESTYLELIGHTING: "IfcSurfaceStyleLighting",
  IFCSURFACESTYLE: "IfcSurfaceStyle",
  IFCSTYLEDREPRESENTATION: "IfcStyledRepresentation",
  IFCSTYLEDITEM: "IfcStyledItem",
  IFCSTYLEMODEL: "IfcStyleModel",
  IFCSTRUCTURALLOADTEMPERATURE: "IfcStructuralLoadTemperature",
  IFCSTRUCTURALLOADSTATIC: "IfcStructuralLoadStatic",
  IFCSTRUCTURALLOAD: "IfcStructuralLoad",
  IFCSTRUCTURALCONNECTIONCONDITION: "IfcStructuralConnectionCondition",
  IFCSIMPLEPROPERTY: "IfcSimpleProperty",
  IFCSHAPEREPRESENTATION: "IfcShapeRepresentation",
  IFCSHAPEMODEL: "IfcShapeModel",
  IFCSHAPEASPECT: "IfcShapeAspect",
  IFCSECTIONREINFORCEMENTPROPERTIES: "IfcSectionReinforcementProperties",
  IFCSECTIONPROPERTIES: "IfcSectionProperties",
  IFCSIUNIT: "IfcSIUnit",
  IFCROOT: "IfcRoot",
  IFCRIBPLATEPROFILEPROPERTIES: "IfcRibPlateProfileProperties",
  IFCREPRESENTATIONMAP: "IfcRepresentationMap",
  IFCREPRESENTATIONITEM: "IfcRepresentationItem",
  IFCREPRESENTATIONCONTEXT: "IfcRepresentationContext",
  IFCREPRESENTATION: "IfcRepresentation",
  IFCRELAXATION: "IfcRelaxation",
  IFCREINFORCEMENTBARPROPERTIES: "IfcReinforcementBarProperties",
  IFCREFERENCESVALUEDOCUMENT: "IfcReferencesValueDocument",
  IFCQUANTITYWEIGHT: "IfcQuantityWeight",
  IFCQUANTITYVOLUME: "IfcQuantityVolume",
  IFCQUANTITYTIME: "IfcQuantityTime",
  IFCQUANTITYLENGTH: "IfcQuantityLength",
  IFCQUANTITYCOUNT: "IfcQuantityCount",
  IFCQUANTITYAREA: "IfcQuantityArea",
  IFCPROPERTYENUMERATION: "IfcPropertyEnumeration",
  IFCPROPERTYDEPENDENCYRELATIONSHIP: "IfcPropertyDependencyRelationship",
  IFCPROPERTYCONSTRAINTRELATIONSHIP: "IfcPropertyConstraintRelationship",
  IFCPROPERTY: "IfcProperty",
  IFCPROFILEPROPERTIES: "IfcProfileProperties",
  IFCPROFILEDEF: "IfcProfileDef",
  IFCPRODUCTSOFCOMBUSTIONPROPERTIES: "IfcProductsOfCombustionProperties",
  IFCPRODUCTREPRESENTATION: "IfcProductRepresentation",
  IFCPRESENTATIONSTYLEASSIGNMENT: "IfcPresentationStyleAssignment",
  IFCPRESENTATIONSTYLE: "IfcPresentationStyle",
  IFCPRESENTATIONLAYERWITHSTYLE: "IfcPresentationLayerWithStyle",
  IFCPRESENTATIONLAYERASSIGNMENT: "IfcPresentationLayerAssignment",
  IFCPREDEFINEDTEXTFONT: "IfcPredefinedTextFont",
  IFCPREDEFINEDTERMINATORSYMBOL: "IfcPredefinedTerminatorSymbol",
  IFCPREDEFINEDSYMBOL: "IfcPredefinedSymbol",
  IFCPREDEFINEDITEM: "IfcPredefinedItem",
  IFCPOSTALADDRESS: "IfcPostalAddress",
  IFCPHYSICALSIMPLEQUANTITY: "IfcPhysicalSimpleQuantity",
  IFCPHYSICALQUANTITY: "IfcPhysicalQuantity",
  IFCPERSONANDORGANIZATION: "IfcPersonAndOrganization",
  IFCPERSON: "IfcPerson",
  IFCOWNERHISTORY: "IfcOwnerHistory",
  IFCORGANIZATIONRELATIONSHIP: "IfcOrganizationRelationship",
  IFCORGANIZATION: "IfcOrganization",
  IFCOPTICALMATERIALPROPERTIES: "IfcOpticalMaterialProperties",
  IFCOBJECTIVE: "IfcObjective",
  IFCOBJECTPLACEMENT: "IfcObjectPlacement",
  IFCNAMEDUNIT: "IfcNamedUnit",
  IFCMONETARYUNIT: "IfcMonetaryUnit",
  IFCMETRIC: "IfcMetric",
  IFCMECHANICALSTEELMATERIALPROPERTIES: "IfcMechanicalSteelMaterialProperties",
  IFCMECHANICALMATERIALPROPERTIES: "IfcMechanicalMaterialProperties",
  IFCMEASUREWITHUNIT: "IfcMeasureWithUnit",
  IFCMATERIALPROPERTIES: "IfcMaterialProperties",
  IFCMATERIALLIST: "IfcMaterialList",
  IFCMATERIALLAYERSETUSAGE: "IfcMaterialLayerSetUsage",
  IFCMATERIALLAYERSET: "IfcMaterialLayerSet",
  IFCMATERIALLAYER: "IfcMaterialLayer",
  IFCMATERIALCLASSIFICATIONRELATIONSHIP: "IfcMaterialClassificationRelationship",
  IFCMATERIAL: "IfcMaterial",
  IFCLOCALTIME: "IfcLocalTime",
  IFCLIGHTINTENSITYDISTRIBUTION: "IfcLightIntensityDistribution",
  IFCLIGHTDISTRIBUTIONDATA: "IfcLightDistributionData",
  IFCLIBRARYREFERENCE: "IfcLibraryReference",
  IFCLIBRARYINFORMATION: "IfcLibraryInformation",
  IFCIRREGULARTIMESERIESVALUE: "IfcIrregularTimeSeriesValue",
  IFCGRIDAXIS: "IfcGridAxis",
  IFCEXTERNALLYDEFINEDTEXTFONT: "IfcExternallyDefinedTextFont",
  IFCEXTERNALLYDEFINEDSYMBOL: "IfcExternallyDefinedSymbol",
  IFCEXTERNALLYDEFINEDSURFACESTYLE: "IfcExternallyDefinedSurfaceStyle",
  IFCEXTERNALLYDEFINEDHATCHSTYLE: "IfcExternallyDefinedHatchStyle",
  IFCEXTERNALREFERENCE: "IfcExternalReference",
  IFCENVIRONMENTALIMPACTVALUE: "IfcEnvironmentalImpactValue",
  IFCDRAUGHTINGCALLOUTRELATIONSHIP: "IfcDraughtingCalloutRelationship",
  IFCDOCUMENTINFORMATIONRELATIONSHIP: "IfcDocumentInformationRelationship",
  IFCDOCUMENTINFORMATION: "IfcDocumentInformation",
  IFCDOCUMENTELECTRONICFORMAT: "IfcDocumentElectronicFormat",
  IFCDIMENSIONALEXPONENTS: "IfcDimensionalExponents",
  IFCDERIVEDUNITELEMENT: "IfcDerivedUnitElement",
  IFCDERIVEDUNIT: "IfcDerivedUnit",
  IFCDATEANDTIME: "IfcDateAndTime",
  IFCCURVESTYLEFONTPATTERN: "IfcCurveStyleFontPattern",
  IFCCURVESTYLEFONTANDSCALING: "IfcCurveStyleFontAndScaling",
  IFCCURVESTYLEFONT: "IfcCurveStyleFont",
  IFCCURRENCYRELATIONSHIP: "IfcCurrencyRelationship",
  IFCCOSTVALUE: "IfcCostValue",
  IFCCOORDINATEDUNIVERSALTIMEOFFSET: "IfcCoordinatedUniversalTimeOffset",
  IFCCONSTRAINTRELATIONSHIP: "IfcConstraintRelationship",
  IFCCONSTRAINTCLASSIFICATIONRELATIONSHIP: "IfcConstraintClassificationRelationship",
  IFCCONSTRAINTAGGREGATIONRELATIONSHIP: "IfcConstraintAggregationRelationship",
  IFCCONSTRAINT: "IfcConstraint",
  IFCCONNECTIONSURFACEGEOMETRY: "IfcConnectionSurfaceGeometry",
  IFCCONNECTIONPORTGEOMETRY: "IfcConnectionPortGeometry",
  IFCCONNECTIONPOINTGEOMETRY: "IfcConnectionPointGeometry",
  IFCCONNECTIONGEOMETRY: "IfcConnectionGeometry",
  IFCCOLOURSPECIFICATION: "IfcColourSpecification",
  IFCCLASSIFICATIONNOTATIONFACET: "IfcClassificationNotationFacet",
  IFCCLASSIFICATIONNOTATION: "IfcClassificationNotation",
  IFCCLASSIFICATIONITEMRELATIONSHIP: "IfcClassificationItemRelationship",
  IFCCLASSIFICATIONITEM: "IfcClassificationItem",
  IFCCLASSIFICATION: "IfcClassification",
  IFCCALENDARDATE: "IfcCalendarDate",
  IFCBOUNDARYNODECONDITIONWARPING: "IfcBoundaryNodeConditionWarping",
  IFCBOUNDARYNODECONDITION: "IfcBoundaryNodeCondition",
  IFCBOUNDARYFACECONDITION: "IfcBoundaryFaceCondition",
  IFCBOUNDARYEDGECONDITION: "IfcBoundaryEdgeCondition",
  IFCBOUNDARYCONDITION: "IfcBoundaryCondition",
  IFCAPPROVALRELATIONSHIP: "IfcApprovalRelationship",
  IFCAPPROVALPROPERTYRELATIONSHIP: "IfcApprovalPropertyRelationship",
  IFCAPPROVALACTORRELATIONSHIP: "IfcApprovalActorRelationship",
  IFCAPPROVAL: "IfcApproval",
  IFCAPPLIEDVALUERELATIONSHIP: "IfcAppliedValueRelationship",
  IFCAPPLIEDVALUE: "IfcAppliedValue",
  IFCAPPLICATION: "IfcApplication",
  IFCADDRESS: "IfcAddress",
  IFCACTORROLE: "IfcActorRole"
};
class Mc {
  constructor() {
    C(this, "factor", 1);
    C(this, "complement", 1);
  }
  apply(i) {
    const e = this.getScaleMatrix().multiply(i);
    i.copy(e);
  }
  setUp(i) {
    var n, r, o;
    this.factor = 1;
    const t = this.getLengthUnits(i);
    if (!t)
      return;
    const e = t == null, s = t.Name === void 0 || t.Name === null;
    e || s || (t.Name.value === "FOOT" && (this.factor = 0.3048), ((n = t.Prefix) == null ? void 0 : n.value) === "MILLI" ? this.complement = 1e-3 : ((r = t.Prefix) == null ? void 0 : r.value) === "CENTI" ? this.complement = 0.01 : ((o = t.Prefix) == null ? void 0 : o.value) === "DECI" && (this.complement = 0.01));
  }
  getLengthUnits(i) {
    try {
      const e = i.GetLineIDsWithType(
        0,
        k.IFCUNITASSIGNMENT
      ).get(0), s = i.GetLine(0, e);
      for (const n of s.Units) {
        if (!n || n.value === null || n.value === void 0)
          continue;
        const r = i.GetLine(0, n.value);
        if (r.UnitType && r.UnitType.value === "LENGTHUNIT")
          return r;
      }
      return null;
    } catch {
      return console.log("Could not get units"), null;
    }
  }
  getScaleMatrix() {
    const i = this.factor;
    return new D.Matrix4().fromArray([
      i,
      0,
      0,
      0,
      0,
      i,
      0,
      0,
      0,
      0,
      i,
      0,
      0,
      0,
      0,
      1
    ]);
  }
}
class ro {
  constructor() {
    C(this, "itemsByFloor", {});
    C(this, "_units", new Mc());
  }
  // TODO: Maybe make this more flexible so that it also support more exotic spatial structures?
  setUp(i) {
    this._units.setUp(i), this.cleanUp();
    try {
      const t = i.GetLineIDsWithType(
        0,
        k.IFCRELCONTAINEDINSPATIALSTRUCTURE
      ), e = /* @__PURE__ */ new Set(), s = i.GetLineIDsWithType(0, k.IFCSPACE);
      for (let l = 0; l < s.size(); l++)
        e.add(s.get(l));
      const n = i.GetLineIDsWithType(0, k.IFCRELAGGREGATES), r = n.size();
      for (let l = 0; l < r; l++) {
        const h = n.get(l), f = i.GetLine(0, h);
        if (!f || !f.RelatingObject || !f.RelatedObjects)
          continue;
        const I = f.RelatingObject.value, u = f.RelatedObjects;
        for (const d of u) {
          const E = d.value;
          e.has(E) && (this.itemsByFloor[E] = I);
        }
      }
      const o = {}, a = t.size();
      for (let l = 0; l < a; l++) {
        const h = t.get(l), f = i.GetLine(0, h);
        if (!f || !f.RelatingStructure || !f.RelatedElements)
          continue;
        const I = f.RelatingStructure.value, u = f.RelatedElements;
        if (e.has(I))
          for (const d of u) {
            o[I] || (o[I] = []);
            const E = d.value;
            o[I].push(E);
          }
        else
          for (const d of u) {
            const E = d.value;
            this.itemsByFloor[E] = I;
          }
      }
      for (const l in o) {
        const h = this.itemsByFloor[l];
        if (h !== void 0) {
          const f = o[l];
          for (const I of f)
            this.itemsByFloor[I] = h;
        }
      }
      for (let l = 0; l < r; l++) {
        const h = n.get(l), f = i.GetLine(0, h);
        if (!f || !f.RelatingObject || !f.RelatedObjects)
          continue;
        const I = f.RelatingObject.value, u = f.RelatedObjects;
        for (const d of u) {
          const E = d.value, T = this.itemsByFloor[I];
          T !== void 0 && (this.itemsByFloor[E] = T);
        }
      }
    } catch {
      console.log("Could not get floors.");
    }
  }
  cleanUp() {
    this.itemsByFloor = {};
  }
}
class zn {
  constructor() {
    /** Whether to extract the IFC properties into a JSON. */
    C(this, "includeProperties", !0);
    /**
     * Generate the geometry for categories that are not included by default,
     * like IFCSPACE.
     */
    C(this, "optionalCategories", [k.IFCSPACE]);
    /** Path of the WASM for [web-ifc](https://github.com/ThatOpen/engine_web-ifc). */
    C(this, "wasm", {
      path: "",
      absolute: !1,
      logLevel: k.LogLevel.LOG_LEVEL_OFF
    });
    /** List of categories that won't be converted to fragments. */
    C(this, "excludedCategories", /* @__PURE__ */ new Set());
    /** Exclusive list of categories that will be converted to fragments. If this contains any category, any other categories will be ignored. */
    C(this, "includedCategories", /* @__PURE__ */ new Set());
    /** Whether to save the absolute location of all IFC items. */
    C(this, "saveLocations", !1);
    /** Loader settings for [web-ifc](https://github.com/ThatOpen/engine_web-ifc). */
    C(this, "webIfc", {
      COORDINATE_TO_ORIGIN: !0
      // OPTIMIZE_PROFILES: true,
    });
    /**
     * Whether to automatically set the path to the WASM file for [web-ifc](https://github.com/ThatOpen/engine_web-ifc).
     * If set to true, the path will be set to the default path of the WASM file.
     * If set to false, the path must be provided manually in the `wasm.path` property.
     * Default value is true.
     */
    C(this, "autoSetWasm", !0);
    /**
     * Custom function to handle the file location for [web-ifc](https://github.com/ThatOpen/engine_web-ifc).
     * This function will be called when [web-ifc](https://github.com/ThatOpen/engine_web-ifc) needs to locate a file.
     * If set to null, the default file location handler will be used.
     *
     * @param url - The URL of the file to locate.
     * @returns The absolute path of the file.
     */
    C(this, "customLocateFileHandler", null);
  }
}
class oo {
  constructor() {
    C(this, "defLineMat", new D.LineBasicMaterial({ color: 16777215 }));
  }
  read(i) {
    const t = i.GetAllAlignments(0), e = i.GetAllCrossSections2D(0), s = i.GetAllCrossSections3D(0), n = {
      IfcAlignment: t,
      IfcCrossSection2D: e,
      IfcCrossSection3D: s
    };
    return this.get(n);
  }
  get(i) {
    if (i.IfcAlignment) {
      const t = /* @__PURE__ */ new Map();
      for (const e of i.IfcAlignment) {
        const s = new Qt.Alignment();
        s.absolute = this.getCurves(e.curve3D, s), s.horizontal = this.getCurves(e.horizontal, s), s.vertical = this.getCurves(e.vertical, s), t.set(t.size, s);
      }
      return { alignments: t, coordinationMatrix: new D.Matrix4() };
    }
  }
  getCurves(i, t) {
    const e = [];
    let s = 0;
    for (const n of i) {
      const r = {};
      if (n.data)
        for (const I of n.data) {
          const [u, d] = I.split(": "), E = parseFloat(d);
          r[u] = E || d;
        }
      const { points: o } = n, a = new Float32Array(o.length * 3);
      for (let I = 0; I < o.length; I++) {
        const { x: u, y: d, z: E } = o[I];
        a[I * 3] = u, a[I * 3 + 1] = d, a[I * 3 + 2] = E || 0;
      }
      const l = new D.BufferAttribute(a, 3), h = new D.EdgesGeometry();
      h.setAttribute("position", l);
      const f = new Qt.CurveMesh(
        s,
        r,
        t,
        h,
        this.defLineMat
      );
      e.push(f.curve), s++;
    }
    return e;
  }
}
class ao {
  getNameInfo(i) {
    var f;
    const t = {}, { arguments: e } = i.GetHeaderLine(0, k.FILE_NAME) || {};
    if (!e)
      return t;
    const [
      s,
      n,
      r,
      o,
      a,
      l,
      h
    ] = e;
    if (s != null && s.value && (t.name = s.value), n != null && n.value && (t.creationDate = new Date(n.value)), r) {
      t.author = {};
      const [I, u] = r;
      I != null && I.value && (t.author.name = I.value), u != null && u.value && (t.author.email = u.value);
    }
    return o && ((f = o[0]) != null && f.value) && (t.organization = o[0].value), a != null && a.value && (t.preprocessorVersion = a == null ? void 0 : a.value), l != null && l.value && (t.originatingSystem = l == null ? void 0 : l.value), h != null && h.value && (t.authorization = h == null ? void 0 : h.value), t;
  }
  getDescriptionInfo(i) {
    var r;
    const t = {}, { arguments: e } = i.GetHeaderLine(0, k.FILE_DESCRIPTION) || {};
    if (!e)
      return t;
    const [s, n] = e;
    if (Array.isArray(s) && ((r = s[0]) != null && r.value)) {
      const o = s[0].value.match(/\[([^\]]+)\]/);
      o && o[1] && (t.viewDefinition = o[1]);
    }
    return n != null && n.value && (t.implementationLevel = n.value), t;
  }
}
class co {
  static get(i, t) {
    const e = [
      k.IFCPROJECT,
      k.IFCSITE,
      k.IFCBUILDING,
      k.IFCBUILDINGSTOREY,
      k.IFCSPACE,
      k.IFCROAD,
      k.IFCFACILITY,
      k.IFCFACILITYPART,
      k.IFCBRIDGE
    ], s = i.data;
    for (const n of e) {
      const r = t.GetLineIDsWithType(0, n), o = r.size();
      for (let a = 0; a < o; a++) {
        const l = r.get(a);
        s.has(l) || s.set(l, [[], [0, n]]);
      }
    }
  }
}
const lo = /* @__PURE__ */ new Set([
  1123145078,
  574549367,
  1675464909,
  2059837836,
  3798115385,
  32440307,
  3125803723,
  3207858831,
  2740243338,
  2624227202,
  4240577450,
  3615266464,
  3724593414,
  220341763,
  477187591,
  1878645084,
  1300840506,
  3303107099,
  1607154358,
  1878645084,
  846575682,
  1351298697,
  2417041796,
  3049322572,
  3331915920,
  1416205885,
  776857604,
  3285139300,
  3958052878,
  2827736869,
  2732653382,
  673634403,
  3448662350,
  4142052618,
  2924175390,
  803316827,
  2556980723,
  1809719519,
  2205249479,
  807026263,
  3737207727,
  1660063152,
  2347385850,
  2705031697,
  3732776249,
  2485617015,
  2611217952,
  1704287377,
  2937912522,
  2770003689,
  1281925730,
  1484403080,
  3448662350,
  4142052618,
  3800577675,
  4006246654,
  3590301190,
  1383045692,
  2775532180,
  2047409740,
  370225590,
  3593883385,
  2665983363,
  4124623270,
  812098782,
  3649129432,
  987898635,
  1105321065,
  3510044353,
  1635779807,
  2603310189,
  3406155212,
  1310608509,
  4261334040,
  2736907675,
  3649129432,
  1136057603,
  1260505505,
  4182860854,
  2713105998,
  2898889636,
  59481748,
  3749851601,
  3486308946,
  3150382593,
  1062206242,
  3264961684,
  15328376,
  1485152156,
  370225590,
  1981873012,
  2859738748,
  45288368,
  2614616156,
  2732653382,
  775493141,
  2147822146,
  2601014836,
  2629017746,
  1186437898,
  2367409068,
  1213902940,
  3632507154,
  3900360178,
  476780140,
  1472233963,
  2804161546,
  3008276851,
  738692330,
  374418227,
  315944413,
  3905492369,
  3570813810,
  2571569899,
  178912537,
  2294589976,
  1437953363,
  2133299955,
  572779678,
  3092502836,
  388784114,
  2624227202,
  1425443689,
  3057273783,
  2347385850,
  1682466193,
  2519244187,
  2839578677,
  3958567839,
  2513912981,
  2830218821,
  427810014
]), Ls = class Ls extends At {
  constructor(t) {
    super(t);
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    t.add(Ls.uuid, this);
  }
  /**
   * Exports all the properties of an IFC into an array of JS objects.
   * @param webIfc The instance of [web-ifc](https://github.com/ThatOpen/engine_web-ifc) to use.
   * @param modelID ID of the IFC model whose properties to extract.
   * @param indirect whether to get the indirect relationships as well.
   * @param recursiveSpatial whether to get the properties of spatial items recursively
   * to make the location data available (e.g. absolute position of building).
   */
  async export(t, e, s = !1, n = !0) {
    const r = {}, o = new Set(t.GetIfcEntityList(e)), a = /* @__PURE__ */ new Set([
      k.IFCPROJECT,
      k.IFCSITE,
      k.IFCBUILDING,
      k.IFCBUILDINGSTOREY,
      k.IFCSPACE
    ]);
    for (const l of a)
      o.add(l);
    for (const l of o) {
      if (lo.has(l))
        continue;
      const h = a.has(l) && n, f = t.GetLineIDsWithType(e, l);
      for (const I of f)
        try {
          const u = t.GetLine(0, I, h, s);
          r[u.expressID] = u;
        } catch {
          console.log(
            `Could not get property ${I}, with recursive ${h} and indirect ${s}.`
          );
        }
    }
    return r;
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(Ls, "uuid", "b32c4332-cd67-436e-ba7f-196646c7a635");
let Nn = Ls;
const bi = class bi extends At {
  constructor(t) {
    super(t);
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * An event triggered when the IFC file starts loading.
     */
    C(this, "onIfcStartedLoading", new j());
    /**
     * An event triggered when the setup process is completed.
     */
    C(this, "onSetup", new j());
    /**
     * The settings for the IfcLoader.
     * It includes options for excluding categories, setting WASM paths, and more.
     */
    C(this, "settings", new zn());
    /**
     * The instance of the Web-IFC library used for handling IFC data.
     */
    C(this, "webIfc", new k.IfcAPI());
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    C(this, "_material", new D.MeshLambertMaterial());
    C(this, "_spatialTree", new ro());
    C(this, "_metaData", new ao());
    C(this, "_fragmentInstances", /* @__PURE__ */ new Map());
    C(this, "_civil", new oo());
    C(this, "_visitedFragments", /* @__PURE__ */ new Map());
    C(this, "_materialT", new D.MeshLambertMaterial({
      transparent: !0,
      opacity: 0.5
    }));
    this.components.add(bi.uuid, this), this.settings.excludedCategories.add(k.IFCOPENINGELEMENT);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.webIfc = null, this.onDisposed.trigger(bi.uuid), this.onDisposed.reset();
  }
  /**
   * Sets up the IfcLoader component with the provided configuration.
   *
   * @param config - Optional configuration settings for the IfcLoader.
   * If not provided, the existing settings will be used.
   *
   * @returns A Promise that resolves when the setup process is completed.
   *
   * @remarks
   * If the `autoSetWasm` option is enabled in the configuration,
   * the method will automatically set the WASM paths for the Web-IFC library.
   *
   * @example
   * ```typescript
   * const ifcLoader = new IfcLoader(components);
   * await ifcLoader.setup({ autoSetWasm: true });
   * ```
   */
  async setup(t) {
    this.settings = { ...this.settings, ...t }, this.settings.autoSetWasm && await this.autoSetWasm(), this.onSetup.trigger();
  }
  /**
   * Loads an IFC file and processes it for 3D visualization.
   *
   * @param data - The Uint8Array containing the IFC file data.
   * @param coordinate - Optional boolean indicating whether to coordinate the loaded IFC data. Default is true.
   *
   * @returns A Promise that resolves to the FragmentsGroup containing the loaded and processed IFC data.
   *
   * @example
   * ```typescript
   * const ifcLoader = components.get(IfcLoader);
   * const group = await ifcLoader.load(ifcData);
   * ```
   */
  async load(t, e = !0, s = "") {
    const n = performance.now();
    this.onIfcStartedLoading.trigger(), await this.readIfcFile(t);
    const r = await this.getAllGeometries();
    r.name = s;
    const a = await this.components.get(Nn).export(this.webIfc, 0);
    r.setLocalProperties(a);
    const l = this.components.get(Tt);
    l.groups.set(r.uuid, r);
    for (const h of r.items)
      l.list.set(h.id, h), h.mesh.uuid = h.id, h.group = r;
    l.onFragmentsLoaded.trigger(r), e && l.coordinate([r]);
    for (const [h] of r.data) {
      const f = a[h];
      if (!f || !f.GlobalId)
        continue;
      const I = f.GlobalId.value || f.GlobalId;
      r.globalToExpressIDs.set(I, h);
    }
    return co.get(r, this.webIfc), this.cleanUp(), console.log(`Streaming the IFC took ${performance.now() - n} ms!`), r;
  }
  /**
   * Reads an IFC file and initializes the Web-IFC library.
   *
   * @param data - The Uint8Array containing the IFC file data.
   *
   * @returns A Promise that resolves when the IFC file is opened and initialized.
   *
   * @remarks
   * This method sets the WASM path and initializes the Web-IFC library based on the provided settings.
   * It also opens the IFC model using the provided data and settings.
   *
   * @example
   * ```typescript
   * const ifcLoader = components.get(IfcLoader);
   * await ifcLoader.readIfcFile(ifcData);
   * ```
   */
  async readIfcFile(t) {
    const { path: e, absolute: s, logLevel: n } = this.settings.wasm;
    return this.webIfc.SetWasmPath(e, s), await this.webIfc.Init(this.settings.customLocateFileHandler || void 0), n && this.webIfc.SetLogLevel(n), this.webIfc.OpenModel(t, this.settings.webIfc);
  }
  /**
   * Cleans up the IfcLoader component by resetting the Web-IFC library,
   * clearing the visited fragments and fragment instances maps, and creating a new instance of the Web-IFC library.
   *
   * @remarks
   * This method is called automatically after using the .load() method, so usually you don't need to use it manually.
   *
   * @example
   * ```typescript
   * const ifcLoader = components.get(IfcLoader);
   * ifcLoader.cleanUp();
   * ```
   */
  cleanUp() {
    try {
      this.webIfc.Dispose();
    } catch {
      console.log("Web-ifc wasn't disposed.");
    }
    this.webIfc = null, this.webIfc = new k.IfcAPI(), this._visitedFragments.clear(), this._fragmentInstances.clear();
  }
  async getAllGeometries() {
    this._spatialTree.setUp(this.webIfc);
    const t = this.webIfc.GetIfcEntityList(0), e = new Qt.FragmentsGroup();
    e.ifcMetadata = {
      name: "",
      description: "",
      ...this._metaData.getNameInfo(this.webIfc),
      ...this._metaData.getDescriptionInfo(this.webIfc),
      schema: this.webIfc.GetModelSchema(0) || "IFC2X3",
      maxExpressID: this.webIfc.GetMaxExpressID(0)
    };
    const s = [];
    for (const r of t) {
      if (!this.webIfc.IsIfcElement(r) && r !== k.IFCSPACE)
        continue;
      const o = this.settings.includedCategories;
      if (o.size > 0 && !o.has(r))
        continue;
      if (this.settings.excludedCategories.has(r))
        continue;
      const a = this.webIfc.GetLineIDsWithType(0, r), l = a.size();
      for (let h = 0; h < l; h++) {
        const f = a.get(h);
        s.push(f);
        const I = this._spatialTree.itemsByFloor[f] || 0;
        e.data.set(f, [[], [I, r]]);
      }
    }
    this._spatialTree.cleanUp(), this.webIfc.StreamMeshes(0, s, (r) => {
      this.getMesh(r, e);
    });
    for (const r of this._visitedFragments) {
      const { index: o, fragment: a } = r[1];
      e.keyFragments.set(o, a.id);
    }
    for (const r of e.items) {
      const o = this._fragmentInstances.get(r.id);
      if (!o)
        throw new Error("Fragment not found!");
      const a = [];
      for (const [l, h] of o)
        a.push(h);
      r.add(a);
    }
    const n = this.webIfc.GetCoordinationMatrix(0);
    return e.coordinationMatrix.fromArray(n), e.civilData = this._civil.read(this.webIfc), e;
  }
  getMesh(t, e) {
    const s = t.geometries.size(), n = t.expressID;
    for (let r = 0; r < s; r++) {
      const o = t.geometries.get(r), { x: a, y: l, z: h, w: f } = o.color, I = f !== 1, { geometryExpressID: u } = o, d = `${u}-${I}`;
      if (!this._visitedFragments.has(d)) {
        const F = this.getGeometry(this.webIfc, u), O = I ? this._materialT : this._material, y = new Qt.Fragment(F, O, 1);
        e.add(y.mesh), e.items.push(y);
        const w = this._visitedFragments.size;
        this._visitedFragments.set(d, { index: w, fragment: y });
      }
      const E = new D.Color().setRGB(a, l, h, "srgb"), T = new D.Matrix4();
      T.fromArray(o.flatTransformation);
      const p = this._visitedFragments.get(d);
      if (p === void 0)
        throw new Error("Error getting geometry data for streaming!");
      const R = e.data.get(n);
      if (!R)
        throw new Error("Data not found!");
      R[0].push(p.index);
      const { fragment: S } = p;
      this._fragmentInstances.has(S.id) || this._fragmentInstances.set(S.id, /* @__PURE__ */ new Map());
      const m = this._fragmentInstances.get(S.id);
      if (!m)
        throw new Error("Instances not found!");
      if (m.has(n)) {
        const F = m.get(n);
        if (!F)
          throw new Error("Instance not found!");
        F.transforms.push(T), F.colors && F.colors.push(E);
      } else
        m.set(n, { id: n, transforms: [T], colors: [E] });
    }
  }
  getGeometry(t, e) {
    const s = t.GetGeometry(0, e), n = t.GetIndexArray(
      s.GetIndexData(),
      s.GetIndexDataSize()
    ), r = t.GetVertexArray(
      s.GetVertexData(),
      s.GetVertexDataSize()
    ), o = new Float32Array(r.length / 2), a = new Float32Array(r.length / 2);
    for (let I = 0; I < r.length; I += 6)
      o[I / 2] = r[I], o[I / 2 + 1] = r[I + 1], o[I / 2 + 2] = r[I + 2], a[I / 2] = r[I + 3], a[I / 2 + 1] = r[I + 4], a[I / 2 + 2] = r[I + 5];
    const l = new D.BufferGeometry(), h = new D.BufferAttribute(o, 3), f = new D.BufferAttribute(a, 3);
    return l.setAttribute("position", h), l.setAttribute("normal", f), l.setIndex(Array.from(n)), s.delete(), l;
  }
  async autoSetWasm() {
    const t = await fetch(
      `https://unpkg.com/@thatopen/components@${Cs.release}/package.json`
    );
    if (!t.ok) {
      console.warn(
        "Couldn't get openbim-components package.json. Set wasm settings manually."
      );
      return;
    }
    const e = await t.json();
    if (!("web-ifc" in e.peerDependencies))
      console.warn(
        "Couldn't get web-ifc from peer dependencies in openbim-components. Set wasm settings manually."
      );
    else {
      const s = e.peerDependencies["web-ifc"];
      this.settings.wasm.path = `https://unpkg.com/web-ifc@${s}/`, this.settings.wasm.absolute = !0;
    }
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(bi, "uuid", "a659add7-1418-4771-a0d6-7d4d438e4624");
let yn = bi;
const Dc = {
  // IfcRelAssigns
  IfcRelAssignsToControl: { related: 5, relating: 7 },
  IfcRelAssignsToGroup: { related: 5, relating: 7 },
  IfcRelAssignsToProduct: { related: 5, relating: 7 },
  // IfcRelAssociates
  IfcRelAssociatesClassification: { related: 5, relating: 6 },
  IfcRelAssociatesMaterial: { related: 5, relating: 6 },
  IfcRelAssociatesDocument: { related: 5, relating: 6 },
  // IfcRelConnects
  IfcRelContainedInSpatialStructure: { related: 5, relating: 6 },
  IfcRelFlowControlElements: { related: 5, relating: 6 },
  IfcRelConnectsElements: { related: 7, relating: 6 },
  // IfcRelDeclares
  IfcRelDeclares: { related: 6, relating: 5 },
  // IfcRelDecomposes
  IfcRelAggregates: { related: 6, relating: 5 },
  IfcRelNests: { related: 6, relating: 5 },
  // IfcRelDefines
  IfcRelDefinesByProperties: { related: 5, relating: 6 },
  IfcRelDefinesByType: { related: 5, relating: 6 },
  IfcRelDefinesByTemplate: { related: 5, relating: 6 }
}, bc = {
  // IfcRelAssigns
  [k.IFCRELASSIGNSTOCONTROL]: "IfcRelAssignsToControl",
  [k.IFCRELASSIGNSTOGROUP]: "IfcRelAssignsToGroup",
  [k.IFCRELASSIGNSTOPRODUCT]: "IfcRelAssignsToProduct",
  // IfcRelAssociates
  [k.IFCRELASSOCIATESCLASSIFICATION]: "IfcRelAssociatesClassification",
  [k.IFCRELASSOCIATESMATERIAL]: "IfcRelAssociatesMaterial",
  [k.IFCRELASSOCIATESDOCUMENT]: "IfcRelAssociatesDocument",
  // IfcRelConnects
  [k.IFCRELCONTAINEDINSPATIALSTRUCTURE]: "IfcRelContainedInSpatialStructure",
  [k.IFCRELCONNECTSELEMENTS]: "IfcRelConnectsElements",
  [k.IFCRELFLOWCONTROLELEMENTS]: "IfcRelFlowControlElements",
  // IfcRelDeclares
  [k.IFCRELDECLARES]: "IfcRelDeclares",
  // IfcRelDecomposes
  [k.IFCRELAGGREGATES]: "IfcRelAggregates",
  [k.IFCRELNESTS]: "IfcRelNests",
  // IfcRelDefines
  [k.IFCRELDEFINESBYPROPERTIES]: "IfcRelDefinesByProperties",
  [k.IFCRELDEFINESBYTYPE]: "IfcRelDefinesByType",
  [k.IFCRELDEFINESBYTEMPLATE]: "IfcRelDefinesByTemplate"
}, me = class me extends At {
  constructor(t) {
    super(t);
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * Event triggered when a file is requested for export.
     */
    C(this, "onRequestFile", new j());
    /**
     * ArrayBuffer containing the IFC data to be exported.
     */
    C(this, "ifcToExport", null);
    /**
     * Event triggered when an element is added to a Pset.
     */
    C(this, "onElementToPset", new j());
    /**
     * Event triggered when a property is added to a Pset.
     */
    C(this, "onPropToPset", new j());
    /**
     * Event triggered when a Pset is removed.
     */
    C(this, "onPsetRemoved", new j());
    /**
     * Event triggered when data in the model changes.
     */
    C(this, "onDataChanged", new j());
    /**
     * Configuration for the WebAssembly module.
     */
    C(this, "wasm", {
      path: "/",
      absolute: !1
    });
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    /**
     * Map of attribute listeners.
     */
    C(this, "attributeListeners", {});
    /**
     * The currently selected model.
     */
    C(this, "selectedModel");
    /**
     * Map of changed entities in the model.
     */
    C(this, "changeMap", {});
    this.components.add(me.uuid, this);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.selectedModel = void 0, this.attributeListeners = {}, this.changeMap = {}, this.onElementToPset.reset(), this.onPropToPset.reset(), this.onPsetRemoved.reset(), this.onDataChanged.reset(), this.onDisposed.trigger(me.uuid), this.onDisposed.reset();
  }
  /**
   * Static method to retrieve the IFC schema from a given model.
   *
   * @param model - The FragmentsGroup model from which to retrieve the IFC schema.
   * @throws Will throw an error if the IFC schema is not found in the model.
   * @returns The IFC schema associated with the given model.
   */
  static getIFCSchema(t) {
    const e = t.ifcMetadata.schema;
    if (!e)
      throw new Error("IFC Schema not found");
    return e.startsWith("IFC2X3") ? "IFC2X3" : e.startsWith("IFC4") && e.replace("IFC4", "") === "" ? "IFC4" : e.startsWith("IFC4X3") ? "IFC4X3" : e;
  }
  /**
   * Method to add or update entity attributes in the model.
   *
   * @param model - The FragmentsGroup model in which to set the properties.
   * @param dataToSave - An array of objects representing the properties to be saved.
   * Each object must have an `expressID` property, which is the express ID of the entity in the model.
   * The rest of the properties will be set as the properties of the entity.
   *
   * @returns A promise that resolves when all the properties have been set.
   *
   * @throws Will throw an error if any of the `expressID` properties are missing in the `dataToSave` array.
   */
  async setData(t, ...e) {
    for (const s of e) {
      const { expressID: n } = s;
      (!n || n === -1) && (s.expressID = this.getNewExpressID(t)), await t.setProperties(s.expressID, s), this.registerChange(t, s.expressID);
    }
  }
  /**
   * Creates a new Property Set (Pset) in the given model.
   *
   * @param model - The FragmentsGroup model in which to create the Pset.
   * @param name - The name of the Pset.
   * @param description - (Optional) The description of the Pset.
   *
   * @returns A promise that resolves with an object containing the newly created Pset and its relation.
   *
   * @throws Will throw an error if the IFC schema is not found in the model.
   * @throws Will throw an error if no OwnerHistory is found in the model.
   */
  async newPset(t, e, s) {
    const n = me.getIFCSchema(t), { handle: r } = await this.getOwnerHistory(t), o = this.newGUID(t), a = new k[n].IfcLabel(e), l = s ? new k[n].IfcText(s) : null, h = new k[n].IfcPropertySet(
      o,
      r,
      a,
      l,
      []
    );
    return h.expressID = this.getNewExpressID(t), await this.setData(t, h), { pset: h };
  }
  /**
   * Removes a Property Set (Pset) from the given model.
   *
   * @param model - The FragmentsGroup model from which to remove the Pset.
   * @param psetID - The express IDs of the Psets to be removed.
   *
   * @returns A promise that resolves when all the Psets have been removed.
   *
   * @throws Will throw an error if any of the `expressID` properties are missing in the `psetID` array.
   * @throws Will throw an error if the Pset to be removed is not of type `IFCPROPERTYSET`.
   * @throws Will throw an error if no relation is found between the Pset and the model.
   */
  async removePset(t, ...e) {
    for (const s of e) {
      const n = await t.getProperties(s);
      if ((n == null ? void 0 : n.type) !== k.IFCPROPERTYSET)
        continue;
      const r = await oi.getPsetRel(t, s);
      if (r && (await t.setProperties(r, null), this.registerChange(t, r)), n) {
        for (const o of n.HasProperties)
          await t.setProperties(o.value, null);
        await t.setProperties(s, null), this.onPsetRemoved.trigger({ model: t, psetID: s }), this.registerChange(t, s);
      }
    }
  }
  /**
   * Creates a new single-value property of type string in the given model.
   *
   * @param model - The FragmentsGroup model in which to create the property.
   * @param type - The type of the property value. Must be a string property type.
   * @param name - The name of the property.
   * @param value - The value of the property. Must be a string.
   *
   * @returns The newly created single-value property.
   *
   * @throws Will throw an error if the IFC schema is not found in the model.
   * @throws Will throw an error if no OwnerHistory is found in the model.
   */
  newSingleStringProperty(t, e, s, n) {
    return this.newSingleProperty(t, e, s, n);
  }
  /**
   * Creates a new single-value property of type numeric in the given model.
   *
   * @param model - The FragmentsGroup model in which to create the property.
   * @param type - The type of the property value. Must be a numeric property type.
   * @param name - The name of the property.
   * @param value - The value of the property. Must be a number.
   *
   * @returns The newly created single-value property.
   *
   * @throws Will throw an error if the IFC schema is not found in the model.
   * @throws Will throw an error if no OwnerHistory is found in the model.
   */
  newSingleNumericProperty(t, e, s, n) {
    return this.newSingleProperty(t, e, s, n);
  }
  /**
   * Creates a new single-value property of type boolean in the given model.
   *
   * @param model - The FragmentsGroup model in which to create the property.
   * @param type - The type of the property value. Must be a boolean property type.
   * @param name - The name of the property.
   * @param value - The value of the property. Must be a boolean.
   *
   * @returns The newly created single-value property.
   *
   * @throws Will throw an error if the IFC schema is not found in the model.
   * @throws Will throw an error if no OwnerHistory is found in the model.
   */
  newSingleBooleanProperty(t, e, s, n) {
    return this.newSingleProperty(t, e, s, n);
  }
  /**
   * Removes a property from a Property Set (Pset) in the given model.
   *
   * @param model - The FragmentsGroup model from which to remove the property.
   * @param psetID - The express ID of the Pset from which to remove the property.
   * @param propID - The express ID of the property to be removed.
   *
   * @returns A promise that resolves when the property has been removed.
   *
   * @throws Will throw an error if the Pset or the property to be removed are not found in the model.
   * @throws Will throw an error if the Pset to be removed is not of type `IFCPROPERTYSET`.
   */
  async removePsetProp(t, e, s) {
    const n = await t.getProperties(e), r = await t.getProperties(s);
    !n || !r || n.type === k.IFCPROPERTYSET && r && (n.HasProperties = n.HasProperties.filter((o) => o.value !== s), await t.setProperties(s, null), this.registerChange(t, e, s));
  }
  /**
   * @deprecated Use indexer.addEntitiesRelation instead. This will be removed in future releases.
   */
  addElementToPset(t, e, ...s) {
    this.components.get(Gt).addEntitiesRelation(
      t,
      e,
      { type: k.IFCRELDEFINESBYPROPERTIES, inv: "DefinesOcurrence" },
      ...s
    );
  }
  /**
   * Adds elements to a Property Set (Pset) in the given model.
   *
   * @param model - The FragmentsGroup model in which to add the elements.
   * @param psetID - The express ID of the Pset to which to add the elements.
   * @param elementID - The express IDs of the elements to be added.
   *
   * @returns A promise that resolves when all the elements have been added.
   *
   * @throws Will throw an error if the Pset or the elements to be added are not found in the model.
   * @throws Will throw an error if the Pset to be added to is not of type `IFCPROPERTYSET`.
   * @throws Will throw an error if no relation is found between the Pset and the model.
   */
  async addPropToPset(t, e, ...s) {
    const n = await t.getProperties(e);
    if (n) {
      for (const r of s) {
        if (n.HasProperties.includes(r))
          continue;
        const o = new k.Handle(r);
        n.HasProperties.push(o), this.onPropToPset.trigger({ model: t, psetID: e, propID: r });
      }
      this.registerChange(t, e);
    }
  }
  /**
   * Creates a new instance of a relationship between entities in the IFC model.
   *
   * @param model - The FragmentsGroup model in which to create the relationship.
   * @param type - The type of the relationship to create.
   * @param relatingID - The express ID of the entity that is related to the other entities.
   * @param relatedIDs - The express IDs of the entities that are related to the relating entity.
   *
   * @returns A promise that resolves with the newly created relationship.
   *
   * @throws Will throw an error if the relationship type is unsupported.
   */
  async createIfcRel(t, e, s, n) {
    const r = bc[e];
    if (!r)
      throw new Error(`IfcPropertiesManager: ${r} is unsoported.`);
    const o = me.getIFCSchema(t), a = Dc[r], l = k[o][r];
    if (!(a && l))
      throw new Error(`IfcPropertiesManager: ${r} is unsoported.`);
    const h = [new k[o].IfcGloballyUniqueId(oe.create())], { related: f, relating: I } = a, d = [...new Set(n)].map(
      (p) => new k.Handle(p)
    ), E = (p, R) => {
      for (let S = p; S < R - 1; S++)
        h.push(null);
    };
    f < I ? (E(1, f), h.push(d), E(f, I), h.push(new k.Handle(s))) : (E(1, I), E(I, f), h.push(new k.Handle(s)), h.push(d));
    const T = new l(...h);
    return await this.setData(t, T), T;
  }
  /**
   * Saves the changes made to the model to a new IFC file.
   *
   * @param model - The FragmentsGroup model from which to save the changes.
   * @param ifcToSaveOn - The Uint8Array representing the original IFC file.
   *
   * @returns A promise that resolves with the modified IFC data as a Uint8Array.
   *
   * @throws Will throw an error if any issues occur during the saving process.
   */
  async saveToIfc(t, e) {
    const s = this.components.get(yn), n = s.webIfc, r = await s.readIfcFile(e);
    await this.components.get(Gt).applyRelationChanges();
    const a = this.changeMap[t.uuid] ?? [];
    for (const h of a) {
      const f = await t.getProperties(h);
      f ? n.WriteLine(r, f) : n.GetLine(r, h) && n.DeleteLine(r, h);
    }
    const l = n.SaveModel(r);
    return s.webIfc.CloseModel(r), s.cleanUp(), l;
  }
  /**
   * Retrieves all the entities of a specific type from the model and returns their express IDs wrapped in Handles.
   * This is used to make references of an entity inside another entity attributes.
   *
   * @param model - The FragmentsGroup model from which to retrieve the entities.
   * @param type - The type of the entities to retrieve. This should be the express ID of the IFC type.
   *
   * @returns A promise that resolves with an array of Handles, each containing the express ID of an entity of the specified type.
   * @returns null if the model doesn't have any entity of that type
   */
  async getEntityRef(t, e) {
    const s = await t.getAllPropertiesOfType(e);
    if (!s)
      return null;
    const n = [];
    for (const r in s) {
      const o = new k.Handle(Number(r));
      n.push(o);
    }
    return n;
  }
  /**
   * Sets an attribute listener for a specific attribute of an entity in the model.
   * The listener will trigger an event whenever the attribute's value changes.
   *
   * @param model - The FragmentsGroup model in which to set the attribute listener.
   * @param expressID - The express ID of the entity for which to set the listener.
   * @param attributeName - The name of the attribute for which to set the listener.
   *
   * @returns The event that will be triggered when the attribute's value changes.
   *
   * @throws Will throw an error if the entity with the given expressID doesn't exist.
   * @throws Will throw an error if the attribute is an array or null, and it can't have a listener.
   * @throws Will throw an error if the attribute has a badly defined handle.
   */
  async setAttributeListener(t, e, s) {
    this.attributeListeners[t.uuid] || (this.attributeListeners[t.uuid] = {});
    const n = this.attributeListeners[t.uuid][e] ? this.attributeListeners[t.uuid][e][s] : null;
    if (n)
      return n;
    const r = await t.getProperties(e);
    if (!r)
      throw new Error(`Entity with expressID ${e} doesn't exists.`);
    const o = r[s];
    if (Array.isArray(o) || !o)
      throw new Error(
        `Attribute ${s} is array or null, and it can't have a listener.`
      );
    const a = o.value;
    if (a === void 0 || a == null)
      throw new Error(`Attribute ${s} has a badly defined handle.`);
    const l = new j();
    return Object.defineProperty(r[s], "value", {
      get() {
        return this._value;
      },
      async set(h) {
        this._value = h, l.trigger(h);
      }
    }), r[s].value = a, this.attributeListeners[t.uuid][e] || (this.attributeListeners[t.uuid][e] = {}), this.attributeListeners[t.uuid][e][s] = l, l;
  }
  getNewExpressID(t) {
    return t.ifcMetadata.maxExpressID++, t.ifcMetadata.maxExpressID;
  }
  newGUID(t) {
    const e = me.getIFCSchema(t);
    return new k[e].IfcGloballyUniqueId(oe.create());
  }
  async getOwnerHistory(t) {
    const e = await t.getAllPropertiesOfType(
      k.IFCOWNERHISTORY
    );
    if (!e)
      throw new Error("No OwnerHistory was found.");
    const s = Object.keys(e).map((o) => parseInt(o, 10)), n = e[s[0]], r = new k.Handle(n.expressID);
    return { entity: n, handle: r };
  }
  registerChange(t, ...e) {
    this.changeMap[t.uuid] || (this.changeMap[t.uuid] = /* @__PURE__ */ new Set());
    for (const s of e)
      this.changeMap[t.uuid].add(s), this.onDataChanged.trigger({ model: t, expressID: s });
  }
  async newSingleProperty(t, e, s, n) {
    const r = me.getIFCSchema(t), o = new k[r].IfcIdentifier(s), a = new k[r][e](n), l = new k[r].IfcPropertySingleValue(
      o,
      null,
      a,
      null
    );
    return l.expressID = this.getNewExpressID(t), await this.setData(t, l), l;
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(me, "uuid", "58c2d9f0-183c-48d6-a402-dfcf5b9a34df");
let Ln = me;
const vi = class vi extends At {
  constructor(t) {
    super(t);
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * Event triggered when relations for a model have been indexed.
     * This event provides the model's UUID and the relations map generated for that model.
     *
     * @property {string} modelID - The UUID of the model for which relations have been indexed.
     * @property {RelationsMap} relationsMap - The relations map generated for the specified model.
     * The map keys are expressIDs of entities, and the values are maps where each key is a relation type ID and its value is an array of expressIDs of entities related through that relation type.
     */
    C(this, "onRelationsIndexed", new j());
    /**
     * Holds the relationship mappings for each model processed by the indexer.
     * The structure is a map where each key is a model's UUID, and the value is another map.
     * This inner map's keys are entity expressIDs, and its values are maps where each key is an index
     * representing a specific relation type, and the value is an array of expressIDs of entities
     * that are related through that relation type. This structure allows for efficient querying
     * of entity relationships within a model.
     */
    C(this, "relationMaps", {});
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    C(this, "_relToAttributesMap", vr);
    C(this, "_inverseAttributes", [
      "IsDecomposedBy",
      "Decomposes",
      "AssociatedTo",
      "HasAssociations",
      "ClassificationForObjects",
      "IsGroupedBy",
      "HasAssignments",
      "IsDefinedBy",
      "DefinesOcurrence",
      "IsTypedBy",
      "Types",
      "Defines",
      "ContainedInStructure",
      "ContainsElements",
      "HasControlElements",
      "AssignedToFlowElement",
      "ConnectedTo",
      "ConnectedFrom",
      "ReferencedBy",
      "Declares",
      "HasContext",
      "Controls",
      "IsNestedBy",
      "Nests",
      "DocumentRefForObjects"
    ]);
    C(this, "_ifcRels", [
      k.IFCRELAGGREGATES,
      k.IFCRELASSOCIATESMATERIAL,
      k.IFCRELASSOCIATESCLASSIFICATION,
      k.IFCRELASSIGNSTOGROUP,
      k.IFCRELDEFINESBYPROPERTIES,
      k.IFCRELDEFINESBYTYPE,
      k.IFCRELDEFINESBYTEMPLATE,
      k.IFCRELCONTAINEDINSPATIALSTRUCTURE,
      k.IFCRELFLOWCONTROLELEMENTS,
      k.IFCRELCONNECTSELEMENTS,
      k.IFCRELASSIGNSTOPRODUCT,
      k.IFCRELDECLARES,
      k.IFCRELASSIGNSTOCONTROL,
      k.IFCRELNESTS,
      k.IFCRELASSOCIATESDOCUMENT
    ]);
    C(this, "onFragmentsDisposed", (t) => {
      delete this.relationMaps[t.groupID];
    });
    // Used to create the corresponding IfcRelationship with the IfcPropertiesManager
    C(this, "_changeMap", {});
    /**
     * An event that is triggered when entities are related in a BIM model.
     * The event provides information about the type of relation, the inverse attribute,
     * the IDs of the entities related, and the IDs of the entities that are being related.
     */
    C(this, "onEntitiesRelated", new j());
    this.components.add(vi.uuid, this), t.get(Tt).onFragmentsDisposed.add(this.onFragmentsDisposed);
  }
  indexRelations(t, e, s, n) {
    const r = Object.keys(e).find(
      (f) => f.startsWith("Relating")
    ), o = Object.keys(e).find(
      (f) => f.startsWith("Related")
    );
    if (!(r && o))
      return;
    const a = e[r].value, l = e[o].map((f) => f.value), h = this.getEntityRelations(
      t,
      a,
      n
    );
    for (const f of l)
      h.push(f);
    for (const f of l)
      this.getEntityRelations(t, f, s).push(a);
  }
  getAttributeIndex(t) {
    const e = this._inverseAttributes.indexOf(t);
    if (e === -1)
      throw new Error(
        `IfcRelationsIndexer: ${t} is not a valid IFC Inverse Attribute name or its not supported yet by this component.`
      );
    return e;
  }
  /**
   * Adds a relation map to the model's relations map.
   *
   * @param model - The `FragmentsGroup` model to which the relation map will be added.
   * @param relationMap - The `RelationsMap` to be added to the model's relations map.
   *
   * @fires onRelationsIndexed - Triggers an event with the model's UUID and the added relation map.
   */
  setRelationMap(t, e) {
    this.relationMaps[t.uuid] = e, this.onRelationsIndexed.trigger({
      modelID: t.uuid,
      relationsMap: e
    });
  }
  /**
   * Processes a given model to index its IFC entities relations based on predefined inverse attributes.
   * This method iterates through each specified inverse attribute, retrieves the corresponding relations,
   * and maps them in a structured way to facilitate quick access to related entities.
   *
   * The process involves querying the model for each relation type associated with the inverse attributes
   * and updating the internal relationMaps with the relationships found. This map is keyed by the model's UUID
   * and contains a nested map where each key is an entity's expressID and its value is another map.
   * This inner map's keys are the indices of the inverse attributes, and its values are arrays of expressIDs
   * of entities that are related through that attribute.
   *
   * @param model The `FragmentsGroup` model to be processed. It must have properties loaded.
   * @returns A promise that resolves to the relations map for the processed model. This map is a detailed
   * representation of the relations indexed by entity expressIDs and relation types.
   * @throws An error if the model does not have properties loaded.
   */
  async process(t, e) {
    if (!t.hasProperties)
      throw new Error("FragmentsGroup properties not found");
    let s = this.relationMaps[t.uuid];
    s || (s = /* @__PURE__ */ new Map(), this.relationMaps[t.uuid] = s);
    const n = t.getLocalProperties();
    if (!n)
      return s;
    const r = (e == null ? void 0 : e.relationsToProcess) ?? this._ifcRels;
    for (const [o, a] of Object.entries(n)) {
      if (!r.includes(a.type))
        continue;
      const l = this._relToAttributesMap.get(a.type);
      if (!l)
        continue;
      const { forRelated: h, forRelating: f } = l;
      this.indexRelations(s, a, h, f);
    }
    return this.setRelationMap(t, s), s;
  }
  /**
   * Processes a given model from a WebIfc API to index its IFC entities relations.
   *
   * @param ifcApi - The WebIfc API instance from which to retrieve the model's properties.
   * @param modelID - The unique identifier of the model within the WebIfc API.
   * @returns A promise that resolves to the relations map for the processed model.
   *          This map is a detailed representation of the relations indexed by entity expressIDs and relation types.
   */
  async processFromWebIfc(t, e) {
    const s = /* @__PURE__ */ new Map();
    for (const n of this._ifcRels) {
      const r = this._relToAttributesMap.get(n);
      if (!r)
        continue;
      const { forRelated: o, forRelating: a } = r, l = t.GetLineIDsWithType(e, n);
      for (let h = 0; h < l.size(); h++) {
        const f = await t.properties.getItemProperties(
          e,
          l.get(h)
        );
        this.indexRelations(s, f, o, a);
      }
    }
    return this.onRelationsIndexed.trigger({
      modelID: e.toString(),
      relationsMap: s
    }), s;
  }
  /**
   * Retrieves the relations of a specific entity within a model based on the given relation name.
   * This method searches the indexed relation maps for the specified model and entity,
   * returning the IDs of related entities if a match is found.
   *
   * @param model The `FragmentsGroup` model containing the entity, or its UUID.
   * @param expressID The unique identifier of the entity within the model.
   * @param attribute The IFC schema inverse attribute of the relation to search for (e.g., "IsDefinedBy", "ContainsElements").
   * @returns An array of express IDs representing the related entities. If the array is empty, no relations were found.
   */
  getEntityRelations(t, e, s) {
    const n = this.getAttributeIndex(s);
    let r;
    if (t instanceof Ws ? r = this.relationMaps[t.uuid] : typeof t == "string" ? r = this.relationMaps[t] : r = t, !r && (t instanceof Ws || typeof t == "string")) {
      r = /* @__PURE__ */ new Map();
      const l = t instanceof Ws ? t.uuid : t;
      this.relationMaps[l] = r;
    }
    let o = r.get(e);
    o || (o = /* @__PURE__ */ new Map(), r.set(e, o));
    let a = o.get(n);
    return a || (a = [], o.set(n, a)), a;
  }
  /**
   * Serializes the relations of a given relation map into a JSON string.
   * This method iterates through the relations in the given map, organizing them into a structured object where each key is an expressID of an entity,
   * and its value is another object mapping relation indices to arrays of related entity expressIDs.
   * The resulting object is then serialized into a JSON string.
   *
   * @param relationMap - The map of relations to be serialized. The map keys are expressIDs of entities, and the values are maps where each key is a relation type ID and its value is an array of expressIDs of entities related through that relation type.
   * @returns A JSON string representing the serialized relations of the given relation map.
   */
  serializeRelations(t) {
    const e = {};
    for (const [s, n] of t.entries()) {
      e[s] || (e[s] = {});
      for (const [r, o] of n.entries())
        e[s][r] = o;
    }
    return JSON.stringify(e);
  }
  /**
   * Serializes the relations of a specific model into a JSON string.
   * This method iterates through the relations indexed for the given model,
   * organizing them into a structured object where each key is an expressID of an entity,
   * and its value is another object mapping relation indices to arrays of related entity expressIDs.
   * The resulting object is then serialized into a JSON string.
   *
   * @param model The `FragmentsGroup` model whose relations are to be serialized.
   * @returns A JSON string representing the serialized relations of the specified model.
   * If the model has no indexed relations, `null` is returned.
   */
  serializeModelRelations(t) {
    const e = this.relationMaps[t.uuid];
    return e ? this.serializeRelations(e) : null;
  }
  /**
   * Serializes all relations of every model processed by the indexer into a JSON string.
   * This method iterates through each model's relations indexed in `relationMaps`, organizing them
   * into a structured JSON object. Each top-level key in this object corresponds to a model's UUID,
   * and its value is another object mapping entity expressIDs to their related entities, categorized
   * by relation types. The structure facilitates easy access to any entity's relations across all models.
   *
   * @returns A JSON string representing the serialized relations of all models processed by the indexer.
   *          If no relations have been indexed, an empty object is returned as a JSON string.
   */
  serializeAllRelations() {
    const t = {};
    for (const e in this.relationMaps) {
      const s = this.relationMaps[e], n = {};
      for (const [r, o] of s.entries()) {
        n[r] || (n[r] = {});
        for (const [a, l] of o.entries())
          n[r][a] = l;
      }
      t[e] = n;
    }
    return JSON.stringify(t);
  }
  /**
   * Converts a JSON string representing relations between entities into a structured map.
   * This method parses the JSON string to reconstruct the relations map that indexes
   * entity relations by their express IDs. The outer map keys are the express IDs of entities,
   * and the values are maps where each key is a relation type ID and its value is an array
   * of express IDs of entities related through that relation type.
   *
   * @param json The JSON string to be parsed into the relations map.
   * @returns A `Map` where the key is the express ID of an entity as a number, and the value
   * is another `Map`. This inner map's key is the relation type ID as a number, and its value
   * is an array of express IDs (as numbers) of entities related through that relation type.
   */
  getRelationsMapFromJSON(t) {
    const e = JSON.parse(t), s = /* @__PURE__ */ new Map();
    for (const n in e) {
      const r = e[n], o = /* @__PURE__ */ new Map();
      for (const a in r)
        o.set(Number(a), r[a]);
      s.set(Number(n), o);
    }
    return s;
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.relationMaps = {}, this.components.get(Tt).onFragmentsDisposed.remove(this.onFragmentsDisposed), this.onDisposed.trigger(vi.uuid), this.onDisposed.reset();
  }
  /**
   * Retrieves the entities within a model that have a specific relation with a given entity.
   *
   * @param model - The BIM model to search for related entities.
   * @param inv - The IFC schema inverse attribute of the relation to search for (e.g., "IsDefinedBy", "ContainsElements").
   * @param expressID - The expressID of the entity within the model.
   *
   * @returns A `Set` with the expressIDs of the entities that have the specified relation with the given entity.
   *
   * @throws An error if the model relations are not indexed or if the inverse attribute name is invalid.
   */
  getEntitiesWithRelation(t, e, s) {
    const n = this.relationMaps[t.uuid];
    if (!n)
      throw new Error(
        "IfcRelationsIndexer: the model relations are not indexed!"
      );
    const r = /* @__PURE__ */ new Set();
    for (const [o, a] of n) {
      const l = this.getAttributeIndex(e), h = a.get(l);
      h && h.includes(s) && r.add(o);
    }
    return r;
  }
  /**
   * Adds relations between an entity and other entities in a BIM model.
   *
   * @param model - The BIM model to which the relations will be added.
   * @param expressID - The expressID of the entity within the model.
   * @param relationName - The IFC schema inverse attribute of the relation to add (e.g., "IsDefinedBy", "ContainsElements").
   * @param relIDs - The expressIDs of the related entities within the model.
   * @deprecated Use addEntitiesRelation instead. This will be removed in future versions.
   *
   * @throws An error if the relation name is not a valid relation name.
   */
  addEntityRelations(t, e, s, ...n) {
    const r = this.getEntityRelations(
      t,
      e,
      s
    );
    if (r)
      r.push(...n);
    else {
      const o = this.getAttributeIndex(s), a = this.relationMaps[t.uuid].get(e);
      a == null || a.set(o, n);
    }
  }
  /**
   * Converts the relations made into actual IFC data.
   *
   * @remarks This function iterates through the changes made to the relations and applies them to the corresponding BIM model.
   * It only make sense to use it if the relations need to be write in the IFC file.
   *
   * @returns A promise that resolves when all the relation changes have been applied.
   */
  async applyRelationChanges() {
    const t = this.components.get(Tt), e = this.components.get(Ln);
    for (const s in this._changeMap) {
      const n = t.groups.get(s);
      if (!n)
        continue;
      const r = this._changeMap[s];
      for (const [o, a] of r)
        for (const [l, h] of a) {
          const { related: f, relID: I } = h;
          if (I) {
            const u = await n.getProperties(I);
            if (!u)
              continue;
            const d = Object.keys(u), E = d.find((p) => p.startsWith("Related")), T = d.find((p) => p.startsWith("Relating"));
            if (!(E && T))
              continue;
            u[E] = [...f].map((p) => new k.Handle(p)), u[T] = new k.Handle(l), await e.setData(n, u);
          } else {
            const u = await e.createIfcRel(
              n,
              o,
              l,
              [...f]
            );
            if (!u)
              continue;
            h.relID = u.expressID;
          }
        }
    }
  }
  addEntitiesRelation(t, e, s, ...n) {
    const { type: r, inv: o } = s;
    let a = this.relationMaps[t.uuid];
    if (a || (a = /* @__PURE__ */ new Map(), this.relationMaps[t.uuid] = a), !this._ifcRels.includes(r))
      return;
    const l = vr.get(r);
    if (!l)
      return;
    const { forRelated: h, forRelating: f } = l;
    if (!(h === o || f === o))
      return;
    let I = this._changeMap[t.uuid];
    I || (I = new re(), this._changeMap[t.uuid] = I);
    const u = f === o ? [e] : n, d = h === o ? [e] : n;
    let E = I.get(r);
    E || (E = new re(), E.onItemSet.add(
      () => this.onEntitiesRelated.trigger({
        invAttribute: o,
        relType: r,
        relatingIDs: u,
        relatedIDs: d
      })
    ), E.onItemUpdated.add(
      () => this.onEntitiesRelated.trigger({
        invAttribute: o,
        relType: r,
        relatingIDs: u,
        relatedIDs: d
      })
    ), I.set(r, E));
    for (const T of u) {
      let p = E.get(T);
      p || (p = { related: new we() }, E.set(T, p)), p.related.add(...d);
    }
    for (const T of u)
      this.getEntityRelations(t, T, f).push(...d);
    for (const T of d)
      this.getEntityRelations(t, T, h).push(...u);
  }
  /**
   * Gets the children of the given element recursively. E.g. in a model with project - site - building - storeys - rooms, passing a storey will include all its children and the children of the rooms contained in it.
   *
   * @param model The BIM model whose children to get.
   * @param expressID The expressID of the item whose children to get.
   * @param found An optional parameter that includes a set of expressIDs where the found element IDs will be added.
   *
   * @returns A `Set` with the expressIDs of the found items.
   */
  getEntityChildren(t, e, s = /* @__PURE__ */ new Set()) {
    if (s.add(e), this.relationMaps[t.uuid] === void 0)
      throw new Error(
        "The provided model has no indices. You have to generate them first."
      );
    const r = this.getEntityRelations(
      t,
      e,
      "IsDecomposedBy"
    );
    if (r)
      for (const a of r)
        this.getEntityChildren(t, a, s);
    const o = this.getEntityRelations(t, e, "ContainsElements");
    if (o)
      for (const a of o)
        this.getEntityChildren(t, a, s);
    return s;
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(vi, "uuid", "23a889ab-83b3-44a4-8bee-ead83438370b");
let Gt = vi;
const Ps = class Ps extends At {
  constructor(t) {
    super(t);
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * Event triggered when fragments are loaded.
     */
    C(this, "onFragmentsLoaded", new j());
    /**
     * Event triggered when fragments are disposed.
     */
    C(this, "onFragmentsDisposed", new j());
    /**
     * DataMap containing all loaded fragments.
     * The key is the fragment's unique identifier, and the value is the fragment itself.
     */
    C(this, "list", new re());
    /**
     * DataMap containing all loaded fragment groups.
     * The key is the group's unique identifier, and the value is the group itself.
     */
    C(this, "groups", new re());
    C(this, "baseCoordinationModel", "");
    C(this, "baseCoordinationMatrix", new D.Matrix4());
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    C(this, "_loader", new Do());
    this.components.add(Ps.uuid, this);
  }
  /**
   * Getter for the meshes of all fragments in the FragmentsManager.
   * It iterates over the fragments in the list and pushes their meshes into an array.
   * @returns {THREE.Mesh[]} An array of THREE.Mesh objects representing the fragments.
   */
  get meshes() {
    const t = [];
    for (const [e, s] of this.list)
      t.push(s.mesh);
    return t;
  }
  /** {@link Disposable.dispose} */
  dispose() {
    for (const [t, e] of this.groups)
      e.dispose(!0);
    this.baseCoordinationModel = "", this.groups.clear(), this.list.clear(), this.onFragmentsLoaded.reset(), this.onFragmentsDisposed.reset(), this.onDisposed.trigger(), this.onDisposed.reset();
  }
  /**
   * Dispose of a specific fragment group.
   * This method removes the group from the groups map, deletes all fragments within the group from the list,
   * disposes of the group, and triggers the onFragmentsDisposed event.
   *
   * @param group - The fragment group to be disposed.
   */
  disposeGroup(t) {
    const { uuid: e } = t, s = [];
    for (const n of t.items)
      s.push(n.id), this.list.delete(n.id);
    t.dispose(!0), this.groups.delete(t.uuid), this.groups.size === 0 && (this.baseCoordinationModel = "", this.baseCoordinationMatrix = new D.Matrix4()), this.onFragmentsDisposed.trigger({
      groupID: e,
      fragmentIDs: s
    });
  }
  /**
   * Loads a binary file that contain fragment geometry.
   * @param data - The binary data to load.
   * @param config - Optional configuration for loading.
   * @param config.isStreamed - Optional setting to determine whether this model is streamed or not.
   * @param config.coordinate - Whether to apply coordinate transformation. Default is true.
   * @param config.properties - Ifc properties to set on the loaded fragments. Not to be used when streaming.
   * @returns The loaded FragmentsGroup.
   */
  load(t, e) {
    const n = { ...{ coordinate: !0 }, ...e }, { coordinate: r, name: o, properties: a, relationsMap: l } = n, h = this._loader.import(t);
    e && (h.isStreamed = e.isStreamed || !1), o && (h.name = o);
    for (const f of h.items)
      f.group = h, this.list.set(f.id, f);
    return r && this.coordinate([h]), this.groups.set(h.uuid, h), a && h.setLocalProperties(a), l && this.components.get(Gt).setRelationMap(h, l), this.onFragmentsLoaded.trigger(h), h;
  }
  /**
   * Export the specified fragmentsgroup to binary data.
   * @param group - the fragments group to be exported.
   * @returns the exported data as binary buffer.
   */
  export(t) {
    return this._loader.export(t);
  }
  /**
   * Gets a map of model IDs to sets of express IDs for the given fragment ID map.
   * @param fragmentIdMap - A map of fragment IDs to their corresponding express IDs.
   * @returns A map of model IDs to sets of express IDs.
   */
  getModelIdMap(t) {
    const e = {};
    for (const s in t) {
      const n = this.list.get(s);
      if (!(n && n.group))
        continue;
      const r = n.group;
      r.uuid in e || (e[r.uuid] = /* @__PURE__ */ new Set());
      const o = t[s];
      for (const a of o)
        e[r.uuid].add(a);
    }
    return e;
  }
  /**
   * Converts a map of model IDs to sets of express IDs to a fragment ID map.
   * @param modelIdMap - A map of model IDs to their corresponding express IDs.
   * @returns A fragment ID map.
   * @remarks
   * This method iterates through the provided model ID map, retrieves the corresponding model from the `groups` map,
   * and then calls the `getFragmentMap` method of the model to obtain a fragment ID map for the given express IDs.
   * The fragment ID maps are then merged into a single map and returned.
   * If a model with a given ID is not found in the `groups` map, the method skips that model and continues with the next one.
   */
  modelIdToFragmentIdMap(t) {
    let e = {};
    for (const s in t) {
      const n = this.groups.get(s);
      if (!n)
        continue;
      const r = t[s], o = n.getFragmentMap(r);
      e = { ...e, ...o };
    }
    return e;
  }
  /**
   * Converts a collection of IFC GUIDs to a fragmentIdMap.
   *
   * @param guids - An iterable collection of global IDs to be converted to a fragment ID map.
   *
   * @returns A fragment ID map, where the keys are fragment IDs and the values are the corresponding express IDs.
   */
  guidToFragmentIdMap(t) {
    const e = {};
    for (const [n, r] of this.groups) {
      n in e || (e[n] = /* @__PURE__ */ new Set());
      for (const o of t) {
        const a = r.globalToExpressIDs.get(o);
        a && e[n].add(a);
      }
    }
    return this.modelIdToFragmentIdMap(e);
  }
  /**
   * Converts a fragment ID map to a collection of IFC GUIDs.
   *
   * @param fragmentIdMap - A fragment ID map to be converted to a collection of IFC GUIDs.
   *
   * @returns An array of IFC GUIDs.
   */
  fragmentIdMapToGuids(t) {
    const e = [], s = this.getModelIdMap(t);
    for (const n in s) {
      const r = this.groups.get(n);
      if (!r)
        continue;
      const o = s[n];
      for (const a of o)
        for (const [l, h] of r.globalToExpressIDs.entries())
          if (h === a) {
            e.push(l);
            break;
          }
    }
    return e;
  }
  /**
   * Applies coordinate transformation to the provided models.
   * If no models are provided, all groups are used.
   * The first model in the list becomes the base model for coordinate transformation.
   * All other models are then transformed to match the base model's coordinate system.
   *
   * @param models - The models to apply coordinate transformation to.
   * If not provided, all models are used.
   */
  coordinate(t = Array.from(this.groups.values())) {
    if (this.baseCoordinationModel.length === 0) {
      const s = t.pop();
      if (!s)
        return;
      this.baseCoordinationModel = s.uuid, this.baseCoordinationMatrix = s.coordinationMatrix.clone();
    }
    if (t.length)
      for (const s of t)
        s.coordinationMatrix.equals(this.baseCoordinationMatrix) || (s.position.set(0, 0, 0), s.rotation.set(0, 0, 0), s.scale.set(1, 1, 1), s.updateMatrix(), this.applyBaseCoordinateSystem(s, s.coordinationMatrix));
  }
  /**
   * Applies the base coordinate system to the provided object.
   *
   * This function takes an object and its original coordinate system as input.
   * It then inverts the original coordinate system and applies the base coordinate system
   * to the object. This ensures that the object's position, rotation, and scale are
   * transformed to match the base coordinate system (which is taken from the first model loaded).
   *
   * @param object - The object to which the base coordinate system will be applied.
   * This should be an instance of THREE.Object3D.
   *
   * @param originalCoordinateSystem - The original coordinate system of the object.
   * This should be a THREE.Matrix4 representing the object's transformation matrix.
   */
  applyBaseCoordinateSystem(t, e) {
    e && t.applyMatrix4(e.clone().invert()), t.applyMatrix4(this.baseCoordinationMatrix);
  }
  /**
   * Creates a copy of the whole model or a part of it.
   *
   * @param model - The model to clone.
   * @param items - Optional - The part of the model to be cloned. If not given, the whole group is cloned.
   *
   */
  clone(t, e) {
    const s = t.cloneGroup(e);
    this.groups.set(s.uuid, s);
    for (const n of s.items)
      this.list.set(n.id, n);
    return s;
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(Ps, "uuid", "fef46874-46a3-461b-8c44-2922ab77c806");
let Tt = Ps;
var us = typeof globalThis < "u" ? globalThis : typeof window < "u" ? window : typeof global < "u" ? global : typeof self < "u" ? self : {};
function vc(c) {
  return c && c.__esModule && Object.prototype.hasOwnProperty.call(c, "default") ? c.default : c;
}
function fs(c) {
  throw new Error('Could not dynamically require "' + c + '". Please configure the dynamicRequireTargets or/and ignoreDynamicRequires option of @rollup/plugin-commonjs appropriately for this require call to work.');
}
var ho = { exports: {} };
/*!

JSZip v3.10.1 - A JavaScript class for generating and reading zip files
<http://stuartk.com/jszip>

(c) 2009-2016 Stuart Knightley <stuart [at] stuartk.com>
Dual licenced under the MIT license or GPLv3. See https://raw.github.com/Stuk/jszip/main/LICENSE.markdown.

JSZip uses the library pako released under the MIT license :
https://github.com/nodeca/pako/blob/main/LICENSE
*/
(function(c, i) {
  (function(t) {
    c.exports = t();
  })(function() {
    return function t(e, s, n) {
      function r(l, h) {
        if (!s[l]) {
          if (!e[l]) {
            var f = typeof fs == "function" && fs;
            if (!h && f)
              return f(l, !0);
            if (o)
              return o(l, !0);
            var I = new Error("Cannot find module '" + l + "'");
            throw I.code = "MODULE_NOT_FOUND", I;
          }
          var u = s[l] = { exports: {} };
          e[l][0].call(u.exports, function(d) {
            var E = e[l][1][d];
            return r(E || d);
          }, u, u.exports, t, e, s, n);
        }
        return s[l].exports;
      }
      for (var o = typeof fs == "function" && fs, a = 0; a < n.length; a++)
        r(n[a]);
      return r;
    }({ 1: [function(t, e, s) {
      var n = t("./utils"), r = t("./support"), o = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
      s.encode = function(a) {
        for (var l, h, f, I, u, d, E, T = [], p = 0, R = a.length, S = R, m = n.getTypeOf(a) !== "string"; p < a.length; )
          S = R - p, f = m ? (l = a[p++], h = p < R ? a[p++] : 0, p < R ? a[p++] : 0) : (l = a.charCodeAt(p++), h = p < R ? a.charCodeAt(p++) : 0, p < R ? a.charCodeAt(p++) : 0), I = l >> 2, u = (3 & l) << 4 | h >> 4, d = 1 < S ? (15 & h) << 2 | f >> 6 : 64, E = 2 < S ? 63 & f : 64, T.push(o.charAt(I) + o.charAt(u) + o.charAt(d) + o.charAt(E));
        return T.join("");
      }, s.decode = function(a) {
        var l, h, f, I, u, d, E = 0, T = 0, p = "data:";
        if (a.substr(0, p.length) === p)
          throw new Error("Invalid base64 input, it looks like a data url.");
        var R, S = 3 * (a = a.replace(/[^A-Za-z0-9+/=]/g, "")).length / 4;
        if (a.charAt(a.length - 1) === o.charAt(64) && S--, a.charAt(a.length - 2) === o.charAt(64) && S--, S % 1 != 0)
          throw new Error("Invalid base64 input, bad content length.");
        for (R = r.uint8array ? new Uint8Array(0 | S) : new Array(0 | S); E < a.length; )
          l = o.indexOf(a.charAt(E++)) << 2 | (I = o.indexOf(a.charAt(E++))) >> 4, h = (15 & I) << 4 | (u = o.indexOf(a.charAt(E++))) >> 2, f = (3 & u) << 6 | (d = o.indexOf(a.charAt(E++))), R[T++] = l, u !== 64 && (R[T++] = h), d !== 64 && (R[T++] = f);
        return R;
      };
    }, { "./support": 30, "./utils": 32 }], 2: [function(t, e, s) {
      var n = t("./external"), r = t("./stream/DataWorker"), o = t("./stream/Crc32Probe"), a = t("./stream/DataLengthProbe");
      function l(h, f, I, u, d) {
        this.compressedSize = h, this.uncompressedSize = f, this.crc32 = I, this.compression = u, this.compressedContent = d;
      }
      l.prototype = { getContentWorker: function() {
        var h = new r(n.Promise.resolve(this.compressedContent)).pipe(this.compression.uncompressWorker()).pipe(new a("data_length")), f = this;
        return h.on("end", function() {
          if (this.streamInfo.data_length !== f.uncompressedSize)
            throw new Error("Bug : uncompressed data size mismatch");
        }), h;
      }, getCompressedWorker: function() {
        return new r(n.Promise.resolve(this.compressedContent)).withStreamInfo("compressedSize", this.compressedSize).withStreamInfo("uncompressedSize", this.uncompressedSize).withStreamInfo("crc32", this.crc32).withStreamInfo("compression", this.compression);
      } }, l.createWorkerFrom = function(h, f, I) {
        return h.pipe(new o()).pipe(new a("uncompressedSize")).pipe(f.compressWorker(I)).pipe(new a("compressedSize")).withStreamInfo("compression", f);
      }, e.exports = l;
    }, { "./external": 6, "./stream/Crc32Probe": 25, "./stream/DataLengthProbe": 26, "./stream/DataWorker": 27 }], 3: [function(t, e, s) {
      var n = t("./stream/GenericWorker");
      s.STORE = { magic: "\0\0", compressWorker: function() {
        return new n("STORE compression");
      }, uncompressWorker: function() {
        return new n("STORE decompression");
      } }, s.DEFLATE = t("./flate");
    }, { "./flate": 7, "./stream/GenericWorker": 28 }], 4: [function(t, e, s) {
      var n = t("./utils"), r = function() {
        for (var o, a = [], l = 0; l < 256; l++) {
          o = l;
          for (var h = 0; h < 8; h++)
            o = 1 & o ? 3988292384 ^ o >>> 1 : o >>> 1;
          a[l] = o;
        }
        return a;
      }();
      e.exports = function(o, a) {
        return o !== void 0 && o.length ? n.getTypeOf(o) !== "string" ? function(l, h, f, I) {
          var u = r, d = I + f;
          l ^= -1;
          for (var E = I; E < d; E++)
            l = l >>> 8 ^ u[255 & (l ^ h[E])];
          return -1 ^ l;
        }(0 | a, o, o.length, 0) : function(l, h, f, I) {
          var u = r, d = I + f;
          l ^= -1;
          for (var E = I; E < d; E++)
            l = l >>> 8 ^ u[255 & (l ^ h.charCodeAt(E))];
          return -1 ^ l;
        }(0 | a, o, o.length, 0) : 0;
      };
    }, { "./utils": 32 }], 5: [function(t, e, s) {
      s.base64 = !1, s.binary = !1, s.dir = !1, s.createFolders = !0, s.date = null, s.compression = null, s.compressionOptions = null, s.comment = null, s.unixPermissions = null, s.dosPermissions = null;
    }, {}], 6: [function(t, e, s) {
      var n = null;
      n = typeof Promise < "u" ? Promise : t("lie"), e.exports = { Promise: n };
    }, { lie: 37 }], 7: [function(t, e, s) {
      var n = typeof Uint8Array < "u" && typeof Uint16Array < "u" && typeof Uint32Array < "u", r = t("pako"), o = t("./utils"), a = t("./stream/GenericWorker"), l = n ? "uint8array" : "array";
      function h(f, I) {
        a.call(this, "FlateWorker/" + f), this._pako = null, this._pakoAction = f, this._pakoOptions = I, this.meta = {};
      }
      s.magic = "\b\0", o.inherits(h, a), h.prototype.processChunk = function(f) {
        this.meta = f.meta, this._pako === null && this._createPako(), this._pako.push(o.transformTo(l, f.data), !1);
      }, h.prototype.flush = function() {
        a.prototype.flush.call(this), this._pako === null && this._createPako(), this._pako.push([], !0);
      }, h.prototype.cleanUp = function() {
        a.prototype.cleanUp.call(this), this._pako = null;
      }, h.prototype._createPako = function() {
        this._pako = new r[this._pakoAction]({ raw: !0, level: this._pakoOptions.level || -1 });
        var f = this;
        this._pako.onData = function(I) {
          f.push({ data: I, meta: f.meta });
        };
      }, s.compressWorker = function(f) {
        return new h("Deflate", f);
      }, s.uncompressWorker = function() {
        return new h("Inflate", {});
      };
    }, { "./stream/GenericWorker": 28, "./utils": 32, pako: 38 }], 8: [function(t, e, s) {
      function n(u, d) {
        var E, T = "";
        for (E = 0; E < d; E++)
          T += String.fromCharCode(255 & u), u >>>= 8;
        return T;
      }
      function r(u, d, E, T, p, R) {
        var S, m, F = u.file, O = u.compression, y = R !== l.utf8encode, w = o.transformTo("string", R(F.name)), L = o.transformTo("string", l.utf8encode(F.name)), b = F.comment, Y = o.transformTo("string", R(b)), N = o.transformTo("string", l.utf8encode(b)), M = L.length !== F.name.length, g = N.length !== b.length, v = "", q = "", G = "", et = F.dir, W = F.date, nt = { crc32: 0, compressedSize: 0, uncompressedSize: 0 };
        d && !E || (nt.crc32 = u.crc32, nt.compressedSize = u.compressedSize, nt.uncompressedSize = u.uncompressedSize);
        var V = 0;
        d && (V |= 8), y || !M && !g || (V |= 2048);
        var x = 0, ot = 0;
        et && (x |= 16), p === "UNIX" ? (ot = 798, x |= function(tt, St) {
          var yt = tt;
          return tt || (yt = St ? 16893 : 33204), (65535 & yt) << 16;
        }(F.unixPermissions, et)) : (ot = 20, x |= function(tt) {
          return 63 & (tt || 0);
        }(F.dosPermissions)), S = W.getUTCHours(), S <<= 6, S |= W.getUTCMinutes(), S <<= 5, S |= W.getUTCSeconds() / 2, m = W.getUTCFullYear() - 1980, m <<= 4, m |= W.getUTCMonth() + 1, m <<= 5, m |= W.getUTCDate(), M && (q = n(1, 1) + n(h(w), 4) + L, v += "up" + n(q.length, 2) + q), g && (G = n(1, 1) + n(h(Y), 4) + N, v += "uc" + n(G.length, 2) + G);
        var it = "";
        return it += `
\0`, it += n(V, 2), it += O.magic, it += n(S, 2), it += n(m, 2), it += n(nt.crc32, 4), it += n(nt.compressedSize, 4), it += n(nt.uncompressedSize, 4), it += n(w.length, 2), it += n(v.length, 2), { fileRecord: f.LOCAL_FILE_HEADER + it + w + v, dirRecord: f.CENTRAL_FILE_HEADER + n(ot, 2) + it + n(Y.length, 2) + "\0\0\0\0" + n(x, 4) + n(T, 4) + w + v + Y };
      }
      var o = t("../utils"), a = t("../stream/GenericWorker"), l = t("../utf8"), h = t("../crc32"), f = t("../signature");
      function I(u, d, E, T) {
        a.call(this, "ZipFileWorker"), this.bytesWritten = 0, this.zipComment = d, this.zipPlatform = E, this.encodeFileName = T, this.streamFiles = u, this.accumulate = !1, this.contentBuffer = [], this.dirRecords = [], this.currentSourceOffset = 0, this.entriesCount = 0, this.currentFile = null, this._sources = [];
      }
      o.inherits(I, a), I.prototype.push = function(u) {
        var d = u.meta.percent || 0, E = this.entriesCount, T = this._sources.length;
        this.accumulate ? this.contentBuffer.push(u) : (this.bytesWritten += u.data.length, a.prototype.push.call(this, { data: u.data, meta: { currentFile: this.currentFile, percent: E ? (d + 100 * (E - T - 1)) / E : 100 } }));
      }, I.prototype.openedSource = function(u) {
        this.currentSourceOffset = this.bytesWritten, this.currentFile = u.file.name;
        var d = this.streamFiles && !u.file.dir;
        if (d) {
          var E = r(u, d, !1, this.currentSourceOffset, this.zipPlatform, this.encodeFileName);
          this.push({ data: E.fileRecord, meta: { percent: 0 } });
        } else
          this.accumulate = !0;
      }, I.prototype.closedSource = function(u) {
        this.accumulate = !1;
        var d = this.streamFiles && !u.file.dir, E = r(u, d, !0, this.currentSourceOffset, this.zipPlatform, this.encodeFileName);
        if (this.dirRecords.push(E.dirRecord), d)
          this.push({ data: function(T) {
            return f.DATA_DESCRIPTOR + n(T.crc32, 4) + n(T.compressedSize, 4) + n(T.uncompressedSize, 4);
          }(u), meta: { percent: 100 } });
        else
          for (this.push({ data: E.fileRecord, meta: { percent: 0 } }); this.contentBuffer.length; )
            this.push(this.contentBuffer.shift());
        this.currentFile = null;
      }, I.prototype.flush = function() {
        for (var u = this.bytesWritten, d = 0; d < this.dirRecords.length; d++)
          this.push({ data: this.dirRecords[d], meta: { percent: 100 } });
        var E = this.bytesWritten - u, T = function(p, R, S, m, F) {
          var O = o.transformTo("string", F(m));
          return f.CENTRAL_DIRECTORY_END + "\0\0\0\0" + n(p, 2) + n(p, 2) + n(R, 4) + n(S, 4) + n(O.length, 2) + O;
        }(this.dirRecords.length, E, u, this.zipComment, this.encodeFileName);
        this.push({ data: T, meta: { percent: 100 } });
      }, I.prototype.prepareNextSource = function() {
        this.previous = this._sources.shift(), this.openedSource(this.previous.streamInfo), this.isPaused ? this.previous.pause() : this.previous.resume();
      }, I.prototype.registerPrevious = function(u) {
        this._sources.push(u);
        var d = this;
        return u.on("data", function(E) {
          d.processChunk(E);
        }), u.on("end", function() {
          d.closedSource(d.previous.streamInfo), d._sources.length ? d.prepareNextSource() : d.end();
        }), u.on("error", function(E) {
          d.error(E);
        }), this;
      }, I.prototype.resume = function() {
        return !!a.prototype.resume.call(this) && (!this.previous && this._sources.length ? (this.prepareNextSource(), !0) : this.previous || this._sources.length || this.generatedError ? void 0 : (this.end(), !0));
      }, I.prototype.error = function(u) {
        var d = this._sources;
        if (!a.prototype.error.call(this, u))
          return !1;
        for (var E = 0; E < d.length; E++)
          try {
            d[E].error(u);
          } catch {
          }
        return !0;
      }, I.prototype.lock = function() {
        a.prototype.lock.call(this);
        for (var u = this._sources, d = 0; d < u.length; d++)
          u[d].lock();
      }, e.exports = I;
    }, { "../crc32": 4, "../signature": 23, "../stream/GenericWorker": 28, "../utf8": 31, "../utils": 32 }], 9: [function(t, e, s) {
      var n = t("../compressions"), r = t("./ZipFileWorker");
      s.generateWorker = function(o, a, l) {
        var h = new r(a.streamFiles, l, a.platform, a.encodeFileName), f = 0;
        try {
          o.forEach(function(I, u) {
            f++;
            var d = function(R, S) {
              var m = R || S, F = n[m];
              if (!F)
                throw new Error(m + " is not a valid compression method !");
              return F;
            }(u.options.compression, a.compression), E = u.options.compressionOptions || a.compressionOptions || {}, T = u.dir, p = u.date;
            u._compressWorker(d, E).withStreamInfo("file", { name: I, dir: T, date: p, comment: u.comment || "", unixPermissions: u.unixPermissions, dosPermissions: u.dosPermissions }).pipe(h);
          }), h.entriesCount = f;
        } catch (I) {
          h.error(I);
        }
        return h;
      };
    }, { "../compressions": 3, "./ZipFileWorker": 8 }], 10: [function(t, e, s) {
      function n() {
        if (!(this instanceof n))
          return new n();
        if (arguments.length)
          throw new Error("The constructor with parameters has been removed in JSZip 3.0, please check the upgrade guide.");
        this.files = /* @__PURE__ */ Object.create(null), this.comment = null, this.root = "", this.clone = function() {
          var r = new n();
          for (var o in this)
            typeof this[o] != "function" && (r[o] = this[o]);
          return r;
        };
      }
      (n.prototype = t("./object")).loadAsync = t("./load"), n.support = t("./support"), n.defaults = t("./defaults"), n.version = "3.10.1", n.loadAsync = function(r, o) {
        return new n().loadAsync(r, o);
      }, n.external = t("./external"), e.exports = n;
    }, { "./defaults": 5, "./external": 6, "./load": 11, "./object": 15, "./support": 30 }], 11: [function(t, e, s) {
      var n = t("./utils"), r = t("./external"), o = t("./utf8"), a = t("./zipEntries"), l = t("./stream/Crc32Probe"), h = t("./nodejsUtils");
      function f(I) {
        return new r.Promise(function(u, d) {
          var E = I.decompressed.getContentWorker().pipe(new l());
          E.on("error", function(T) {
            d(T);
          }).on("end", function() {
            E.streamInfo.crc32 !== I.decompressed.crc32 ? d(new Error("Corrupted zip : CRC32 mismatch")) : u();
          }).resume();
        });
      }
      e.exports = function(I, u) {
        var d = this;
        return u = n.extend(u || {}, { base64: !1, checkCRC32: !1, optimizedBinaryString: !1, createFolders: !1, decodeFileName: o.utf8decode }), h.isNode && h.isStream(I) ? r.Promise.reject(new Error("JSZip can't accept a stream when loading a zip file.")) : n.prepareContent("the loaded zip file", I, !0, u.optimizedBinaryString, u.base64).then(function(E) {
          var T = new a(u);
          return T.load(E), T;
        }).then(function(E) {
          var T = [r.Promise.resolve(E)], p = E.files;
          if (u.checkCRC32)
            for (var R = 0; R < p.length; R++)
              T.push(f(p[R]));
          return r.Promise.all(T);
        }).then(function(E) {
          for (var T = E.shift(), p = T.files, R = 0; R < p.length; R++) {
            var S = p[R], m = S.fileNameStr, F = n.resolve(S.fileNameStr);
            d.file(F, S.decompressed, { binary: !0, optimizedBinaryString: !0, date: S.date, dir: S.dir, comment: S.fileCommentStr.length ? S.fileCommentStr : null, unixPermissions: S.unixPermissions, dosPermissions: S.dosPermissions, createFolders: u.createFolders }), S.dir || (d.file(F).unsafeOriginalName = m);
          }
          return T.zipComment.length && (d.comment = T.zipComment), d;
        });
      };
    }, { "./external": 6, "./nodejsUtils": 14, "./stream/Crc32Probe": 25, "./utf8": 31, "./utils": 32, "./zipEntries": 33 }], 12: [function(t, e, s) {
      var n = t("../utils"), r = t("../stream/GenericWorker");
      function o(a, l) {
        r.call(this, "Nodejs stream input adapter for " + a), this._upstreamEnded = !1, this._bindStream(l);
      }
      n.inherits(o, r), o.prototype._bindStream = function(a) {
        var l = this;
        (this._stream = a).pause(), a.on("data", function(h) {
          l.push({ data: h, meta: { percent: 0 } });
        }).on("error", function(h) {
          l.isPaused ? this.generatedError = h : l.error(h);
        }).on("end", function() {
          l.isPaused ? l._upstreamEnded = !0 : l.end();
        });
      }, o.prototype.pause = function() {
        return !!r.prototype.pause.call(this) && (this._stream.pause(), !0);
      }, o.prototype.resume = function() {
        return !!r.prototype.resume.call(this) && (this._upstreamEnded ? this.end() : this._stream.resume(), !0);
      }, e.exports = o;
    }, { "../stream/GenericWorker": 28, "../utils": 32 }], 13: [function(t, e, s) {
      var n = t("readable-stream").Readable;
      function r(o, a, l) {
        n.call(this, a), this._helper = o;
        var h = this;
        o.on("data", function(f, I) {
          h.push(f) || h._helper.pause(), l && l(I);
        }).on("error", function(f) {
          h.emit("error", f);
        }).on("end", function() {
          h.push(null);
        });
      }
      t("../utils").inherits(r, n), r.prototype._read = function() {
        this._helper.resume();
      }, e.exports = r;
    }, { "../utils": 32, "readable-stream": 16 }], 14: [function(t, e, s) {
      e.exports = { isNode: typeof Buffer < "u", newBufferFrom: function(n, r) {
        if (Buffer.from && Buffer.from !== Uint8Array.from)
          return Buffer.from(n, r);
        if (typeof n == "number")
          throw new Error('The "data" argument must not be a number');
        return new Buffer(n, r);
      }, allocBuffer: function(n) {
        if (Buffer.alloc)
          return Buffer.alloc(n);
        var r = new Buffer(n);
        return r.fill(0), r;
      }, isBuffer: function(n) {
        return Buffer.isBuffer(n);
      }, isStream: function(n) {
        return n && typeof n.on == "function" && typeof n.pause == "function" && typeof n.resume == "function";
      } };
    }, {}], 15: [function(t, e, s) {
      function n(F, O, y) {
        var w, L = o.getTypeOf(O), b = o.extend(y || {}, h);
        b.date = b.date || /* @__PURE__ */ new Date(), b.compression !== null && (b.compression = b.compression.toUpperCase()), typeof b.unixPermissions == "string" && (b.unixPermissions = parseInt(b.unixPermissions, 8)), b.unixPermissions && 16384 & b.unixPermissions && (b.dir = !0), b.dosPermissions && 16 & b.dosPermissions && (b.dir = !0), b.dir && (F = p(F)), b.createFolders && (w = T(F)) && R.call(this, w, !0);
        var Y = L === "string" && b.binary === !1 && b.base64 === !1;
        y && y.binary !== void 0 || (b.binary = !Y), (O instanceof f && O.uncompressedSize === 0 || b.dir || !O || O.length === 0) && (b.base64 = !1, b.binary = !0, O = "", b.compression = "STORE", L = "string");
        var N = null;
        N = O instanceof f || O instanceof a ? O : d.isNode && d.isStream(O) ? new E(F, O) : o.prepareContent(F, O, b.binary, b.optimizedBinaryString, b.base64);
        var M = new I(F, N, b);
        this.files[F] = M;
      }
      var r = t("./utf8"), o = t("./utils"), a = t("./stream/GenericWorker"), l = t("./stream/StreamHelper"), h = t("./defaults"), f = t("./compressedObject"), I = t("./zipObject"), u = t("./generate"), d = t("./nodejsUtils"), E = t("./nodejs/NodejsStreamInputAdapter"), T = function(F) {
        F.slice(-1) === "/" && (F = F.substring(0, F.length - 1));
        var O = F.lastIndexOf("/");
        return 0 < O ? F.substring(0, O) : "";
      }, p = function(F) {
        return F.slice(-1) !== "/" && (F += "/"), F;
      }, R = function(F, O) {
        return O = O !== void 0 ? O : h.createFolders, F = p(F), this.files[F] || n.call(this, F, null, { dir: !0, createFolders: O }), this.files[F];
      };
      function S(F) {
        return Object.prototype.toString.call(F) === "[object RegExp]";
      }
      var m = { load: function() {
        throw new Error("This method has been removed in JSZip 3.0, please check the upgrade guide.");
      }, forEach: function(F) {
        var O, y, w;
        for (O in this.files)
          w = this.files[O], (y = O.slice(this.root.length, O.length)) && O.slice(0, this.root.length) === this.root && F(y, w);
      }, filter: function(F) {
        var O = [];
        return this.forEach(function(y, w) {
          F(y, w) && O.push(w);
        }), O;
      }, file: function(F, O, y) {
        if (arguments.length !== 1)
          return F = this.root + F, n.call(this, F, O, y), this;
        if (S(F)) {
          var w = F;
          return this.filter(function(b, Y) {
            return !Y.dir && w.test(b);
          });
        }
        var L = this.files[this.root + F];
        return L && !L.dir ? L : null;
      }, folder: function(F) {
        if (!F)
          return this;
        if (S(F))
          return this.filter(function(L, b) {
            return b.dir && F.test(L);
          });
        var O = this.root + F, y = R.call(this, O), w = this.clone();
        return w.root = y.name, w;
      }, remove: function(F) {
        F = this.root + F;
        var O = this.files[F];
        if (O || (F.slice(-1) !== "/" && (F += "/"), O = this.files[F]), O && !O.dir)
          delete this.files[F];
        else
          for (var y = this.filter(function(L, b) {
            return b.name.slice(0, F.length) === F;
          }), w = 0; w < y.length; w++)
            delete this.files[y[w].name];
        return this;
      }, generate: function() {
        throw new Error("This method has been removed in JSZip 3.0, please check the upgrade guide.");
      }, generateInternalStream: function(F) {
        var O, y = {};
        try {
          if ((y = o.extend(F || {}, { streamFiles: !1, compression: "STORE", compressionOptions: null, type: "", platform: "DOS", comment: null, mimeType: "application/zip", encodeFileName: r.utf8encode })).type = y.type.toLowerCase(), y.compression = y.compression.toUpperCase(), y.type === "binarystring" && (y.type = "string"), !y.type)
            throw new Error("No output type specified.");
          o.checkSupport(y.type), y.platform !== "darwin" && y.platform !== "freebsd" && y.platform !== "linux" && y.platform !== "sunos" || (y.platform = "UNIX"), y.platform === "win32" && (y.platform = "DOS");
          var w = y.comment || this.comment || "";
          O = u.generateWorker(this, y, w);
        } catch (L) {
          (O = new a("error")).error(L);
        }
        return new l(O, y.type || "string", y.mimeType);
      }, generateAsync: function(F, O) {
        return this.generateInternalStream(F).accumulate(O);
      }, generateNodeStream: function(F, O) {
        return (F = F || {}).type || (F.type = "nodebuffer"), this.generateInternalStream(F).toNodejsStream(O);
      } };
      e.exports = m;
    }, { "./compressedObject": 2, "./defaults": 5, "./generate": 9, "./nodejs/NodejsStreamInputAdapter": 12, "./nodejsUtils": 14, "./stream/GenericWorker": 28, "./stream/StreamHelper": 29, "./utf8": 31, "./utils": 32, "./zipObject": 35 }], 16: [function(t, e, s) {
      e.exports = t("stream");
    }, { stream: void 0 }], 17: [function(t, e, s) {
      var n = t("./DataReader");
      function r(o) {
        n.call(this, o);
        for (var a = 0; a < this.data.length; a++)
          o[a] = 255 & o[a];
      }
      t("../utils").inherits(r, n), r.prototype.byteAt = function(o) {
        return this.data[this.zero + o];
      }, r.prototype.lastIndexOfSignature = function(o) {
        for (var a = o.charCodeAt(0), l = o.charCodeAt(1), h = o.charCodeAt(2), f = o.charCodeAt(3), I = this.length - 4; 0 <= I; --I)
          if (this.data[I] === a && this.data[I + 1] === l && this.data[I + 2] === h && this.data[I + 3] === f)
            return I - this.zero;
        return -1;
      }, r.prototype.readAndCheckSignature = function(o) {
        var a = o.charCodeAt(0), l = o.charCodeAt(1), h = o.charCodeAt(2), f = o.charCodeAt(3), I = this.readData(4);
        return a === I[0] && l === I[1] && h === I[2] && f === I[3];
      }, r.prototype.readData = function(o) {
        if (this.checkOffset(o), o === 0)
          return [];
        var a = this.data.slice(this.zero + this.index, this.zero + this.index + o);
        return this.index += o, a;
      }, e.exports = r;
    }, { "../utils": 32, "./DataReader": 18 }], 18: [function(t, e, s) {
      var n = t("../utils");
      function r(o) {
        this.data = o, this.length = o.length, this.index = 0, this.zero = 0;
      }
      r.prototype = { checkOffset: function(o) {
        this.checkIndex(this.index + o);
      }, checkIndex: function(o) {
        if (this.length < this.zero + o || o < 0)
          throw new Error("End of data reached (data length = " + this.length + ", asked index = " + o + "). Corrupted zip ?");
      }, setIndex: function(o) {
        this.checkIndex(o), this.index = o;
      }, skip: function(o) {
        this.setIndex(this.index + o);
      }, byteAt: function() {
      }, readInt: function(o) {
        var a, l = 0;
        for (this.checkOffset(o), a = this.index + o - 1; a >= this.index; a--)
          l = (l << 8) + this.byteAt(a);
        return this.index += o, l;
      }, readString: function(o) {
        return n.transformTo("string", this.readData(o));
      }, readData: function() {
      }, lastIndexOfSignature: function() {
      }, readAndCheckSignature: function() {
      }, readDate: function() {
        var o = this.readInt(4);
        return new Date(Date.UTC(1980 + (o >> 25 & 127), (o >> 21 & 15) - 1, o >> 16 & 31, o >> 11 & 31, o >> 5 & 63, (31 & o) << 1));
      } }, e.exports = r;
    }, { "../utils": 32 }], 19: [function(t, e, s) {
      var n = t("./Uint8ArrayReader");
      function r(o) {
        n.call(this, o);
      }
      t("../utils").inherits(r, n), r.prototype.readData = function(o) {
        this.checkOffset(o);
        var a = this.data.slice(this.zero + this.index, this.zero + this.index + o);
        return this.index += o, a;
      }, e.exports = r;
    }, { "../utils": 32, "./Uint8ArrayReader": 21 }], 20: [function(t, e, s) {
      var n = t("./DataReader");
      function r(o) {
        n.call(this, o);
      }
      t("../utils").inherits(r, n), r.prototype.byteAt = function(o) {
        return this.data.charCodeAt(this.zero + o);
      }, r.prototype.lastIndexOfSignature = function(o) {
        return this.data.lastIndexOf(o) - this.zero;
      }, r.prototype.readAndCheckSignature = function(o) {
        return o === this.readData(4);
      }, r.prototype.readData = function(o) {
        this.checkOffset(o);
        var a = this.data.slice(this.zero + this.index, this.zero + this.index + o);
        return this.index += o, a;
      }, e.exports = r;
    }, { "../utils": 32, "./DataReader": 18 }], 21: [function(t, e, s) {
      var n = t("./ArrayReader");
      function r(o) {
        n.call(this, o);
      }
      t("../utils").inherits(r, n), r.prototype.readData = function(o) {
        if (this.checkOffset(o), o === 0)
          return new Uint8Array(0);
        var a = this.data.subarray(this.zero + this.index, this.zero + this.index + o);
        return this.index += o, a;
      }, e.exports = r;
    }, { "../utils": 32, "./ArrayReader": 17 }], 22: [function(t, e, s) {
      var n = t("../utils"), r = t("../support"), o = t("./ArrayReader"), a = t("./StringReader"), l = t("./NodeBufferReader"), h = t("./Uint8ArrayReader");
      e.exports = function(f) {
        var I = n.getTypeOf(f);
        return n.checkSupport(I), I !== "string" || r.uint8array ? I === "nodebuffer" ? new l(f) : r.uint8array ? new h(n.transformTo("uint8array", f)) : new o(n.transformTo("array", f)) : new a(f);
      };
    }, { "../support": 30, "../utils": 32, "./ArrayReader": 17, "./NodeBufferReader": 19, "./StringReader": 20, "./Uint8ArrayReader": 21 }], 23: [function(t, e, s) {
      s.LOCAL_FILE_HEADER = "PK", s.CENTRAL_FILE_HEADER = "PK", s.CENTRAL_DIRECTORY_END = "PK", s.ZIP64_CENTRAL_DIRECTORY_LOCATOR = "PK\x07", s.ZIP64_CENTRAL_DIRECTORY_END = "PK", s.DATA_DESCRIPTOR = "PK\x07\b";
    }, {}], 24: [function(t, e, s) {
      var n = t("./GenericWorker"), r = t("../utils");
      function o(a) {
        n.call(this, "ConvertWorker to " + a), this.destType = a;
      }
      r.inherits(o, n), o.prototype.processChunk = function(a) {
        this.push({ data: r.transformTo(this.destType, a.data), meta: a.meta });
      }, e.exports = o;
    }, { "../utils": 32, "./GenericWorker": 28 }], 25: [function(t, e, s) {
      var n = t("./GenericWorker"), r = t("../crc32");
      function o() {
        n.call(this, "Crc32Probe"), this.withStreamInfo("crc32", 0);
      }
      t("../utils").inherits(o, n), o.prototype.processChunk = function(a) {
        this.streamInfo.crc32 = r(a.data, this.streamInfo.crc32 || 0), this.push(a);
      }, e.exports = o;
    }, { "../crc32": 4, "../utils": 32, "./GenericWorker": 28 }], 26: [function(t, e, s) {
      var n = t("../utils"), r = t("./GenericWorker");
      function o(a) {
        r.call(this, "DataLengthProbe for " + a), this.propName = a, this.withStreamInfo(a, 0);
      }
      n.inherits(o, r), o.prototype.processChunk = function(a) {
        if (a) {
          var l = this.streamInfo[this.propName] || 0;
          this.streamInfo[this.propName] = l + a.data.length;
        }
        r.prototype.processChunk.call(this, a);
      }, e.exports = o;
    }, { "../utils": 32, "./GenericWorker": 28 }], 27: [function(t, e, s) {
      var n = t("../utils"), r = t("./GenericWorker");
      function o(a) {
        r.call(this, "DataWorker");
        var l = this;
        this.dataIsReady = !1, this.index = 0, this.max = 0, this.data = null, this.type = "", this._tickScheduled = !1, a.then(function(h) {
          l.dataIsReady = !0, l.data = h, l.max = h && h.length || 0, l.type = n.getTypeOf(h), l.isPaused || l._tickAndRepeat();
        }, function(h) {
          l.error(h);
        });
      }
      n.inherits(o, r), o.prototype.cleanUp = function() {
        r.prototype.cleanUp.call(this), this.data = null;
      }, o.prototype.resume = function() {
        return !!r.prototype.resume.call(this) && (!this._tickScheduled && this.dataIsReady && (this._tickScheduled = !0, n.delay(this._tickAndRepeat, [], this)), !0);
      }, o.prototype._tickAndRepeat = function() {
        this._tickScheduled = !1, this.isPaused || this.isFinished || (this._tick(), this.isFinished || (n.delay(this._tickAndRepeat, [], this), this._tickScheduled = !0));
      }, o.prototype._tick = function() {
        if (this.isPaused || this.isFinished)
          return !1;
        var a = null, l = Math.min(this.max, this.index + 16384);
        if (this.index >= this.max)
          return this.end();
        switch (this.type) {
          case "string":
            a = this.data.substring(this.index, l);
            break;
          case "uint8array":
            a = this.data.subarray(this.index, l);
            break;
          case "array":
          case "nodebuffer":
            a = this.data.slice(this.index, l);
        }
        return this.index = l, this.push({ data: a, meta: { percent: this.max ? this.index / this.max * 100 : 0 } });
      }, e.exports = o;
    }, { "../utils": 32, "./GenericWorker": 28 }], 28: [function(t, e, s) {
      function n(r) {
        this.name = r || "default", this.streamInfo = {}, this.generatedError = null, this.extraStreamInfo = {}, this.isPaused = !0, this.isFinished = !1, this.isLocked = !1, this._listeners = { data: [], end: [], error: [] }, this.previous = null;
      }
      n.prototype = { push: function(r) {
        this.emit("data", r);
      }, end: function() {
        if (this.isFinished)
          return !1;
        this.flush();
        try {
          this.emit("end"), this.cleanUp(), this.isFinished = !0;
        } catch (r) {
          this.emit("error", r);
        }
        return !0;
      }, error: function(r) {
        return !this.isFinished && (this.isPaused ? this.generatedError = r : (this.isFinished = !0, this.emit("error", r), this.previous && this.previous.error(r), this.cleanUp()), !0);
      }, on: function(r, o) {
        return this._listeners[r].push(o), this;
      }, cleanUp: function() {
        this.streamInfo = this.generatedError = this.extraStreamInfo = null, this._listeners = [];
      }, emit: function(r, o) {
        if (this._listeners[r])
          for (var a = 0; a < this._listeners[r].length; a++)
            this._listeners[r][a].call(this, o);
      }, pipe: function(r) {
        return r.registerPrevious(this);
      }, registerPrevious: function(r) {
        if (this.isLocked)
          throw new Error("The stream '" + this + "' has already been used.");
        this.streamInfo = r.streamInfo, this.mergeStreamInfo(), this.previous = r;
        var o = this;
        return r.on("data", function(a) {
          o.processChunk(a);
        }), r.on("end", function() {
          o.end();
        }), r.on("error", function(a) {
          o.error(a);
        }), this;
      }, pause: function() {
        return !this.isPaused && !this.isFinished && (this.isPaused = !0, this.previous && this.previous.pause(), !0);
      }, resume: function() {
        if (!this.isPaused || this.isFinished)
          return !1;
        var r = this.isPaused = !1;
        return this.generatedError && (this.error(this.generatedError), r = !0), this.previous && this.previous.resume(), !r;
      }, flush: function() {
      }, processChunk: function(r) {
        this.push(r);
      }, withStreamInfo: function(r, o) {
        return this.extraStreamInfo[r] = o, this.mergeStreamInfo(), this;
      }, mergeStreamInfo: function() {
        for (var r in this.extraStreamInfo)
          Object.prototype.hasOwnProperty.call(this.extraStreamInfo, r) && (this.streamInfo[r] = this.extraStreamInfo[r]);
      }, lock: function() {
        if (this.isLocked)
          throw new Error("The stream '" + this + "' has already been used.");
        this.isLocked = !0, this.previous && this.previous.lock();
      }, toString: function() {
        var r = "Worker " + this.name;
        return this.previous ? this.previous + " -> " + r : r;
      } }, e.exports = n;
    }, {}], 29: [function(t, e, s) {
      var n = t("../utils"), r = t("./ConvertWorker"), o = t("./GenericWorker"), a = t("../base64"), l = t("../support"), h = t("../external"), f = null;
      if (l.nodestream)
        try {
          f = t("../nodejs/NodejsStreamOutputAdapter");
        } catch {
        }
      function I(d, E) {
        return new h.Promise(function(T, p) {
          var R = [], S = d._internalType, m = d._outputType, F = d._mimeType;
          d.on("data", function(O, y) {
            R.push(O), E && E(y);
          }).on("error", function(O) {
            R = [], p(O);
          }).on("end", function() {
            try {
              var O = function(y, w, L) {
                switch (y) {
                  case "blob":
                    return n.newBlob(n.transformTo("arraybuffer", w), L);
                  case "base64":
                    return a.encode(w);
                  default:
                    return n.transformTo(y, w);
                }
              }(m, function(y, w) {
                var L, b = 0, Y = null, N = 0;
                for (L = 0; L < w.length; L++)
                  N += w[L].length;
                switch (y) {
                  case "string":
                    return w.join("");
                  case "array":
                    return Array.prototype.concat.apply([], w);
                  case "uint8array":
                    for (Y = new Uint8Array(N), L = 0; L < w.length; L++)
                      Y.set(w[L], b), b += w[L].length;
                    return Y;
                  case "nodebuffer":
                    return Buffer.concat(w);
                  default:
                    throw new Error("concat : unsupported type '" + y + "'");
                }
              }(S, R), F);
              T(O);
            } catch (y) {
              p(y);
            }
            R = [];
          }).resume();
        });
      }
      function u(d, E, T) {
        var p = E;
        switch (E) {
          case "blob":
          case "arraybuffer":
            p = "uint8array";
            break;
          case "base64":
            p = "string";
        }
        try {
          this._internalType = p, this._outputType = E, this._mimeType = T, n.checkSupport(p), this._worker = d.pipe(new r(p)), d.lock();
        } catch (R) {
          this._worker = new o("error"), this._worker.error(R);
        }
      }
      u.prototype = { accumulate: function(d) {
        return I(this, d);
      }, on: function(d, E) {
        var T = this;
        return d === "data" ? this._worker.on(d, function(p) {
          E.call(T, p.data, p.meta);
        }) : this._worker.on(d, function() {
          n.delay(E, arguments, T);
        }), this;
      }, resume: function() {
        return n.delay(this._worker.resume, [], this._worker), this;
      }, pause: function() {
        return this._worker.pause(), this;
      }, toNodejsStream: function(d) {
        if (n.checkSupport("nodestream"), this._outputType !== "nodebuffer")
          throw new Error(this._outputType + " is not supported by this method");
        return new f(this, { objectMode: this._outputType !== "nodebuffer" }, d);
      } }, e.exports = u;
    }, { "../base64": 1, "../external": 6, "../nodejs/NodejsStreamOutputAdapter": 13, "../support": 30, "../utils": 32, "./ConvertWorker": 24, "./GenericWorker": 28 }], 30: [function(t, e, s) {
      if (s.base64 = !0, s.array = !0, s.string = !0, s.arraybuffer = typeof ArrayBuffer < "u" && typeof Uint8Array < "u", s.nodebuffer = typeof Buffer < "u", s.uint8array = typeof Uint8Array < "u", typeof ArrayBuffer > "u")
        s.blob = !1;
      else {
        var n = new ArrayBuffer(0);
        try {
          s.blob = new Blob([n], { type: "application/zip" }).size === 0;
        } catch {
          try {
            var r = new (self.BlobBuilder || self.WebKitBlobBuilder || self.MozBlobBuilder || self.MSBlobBuilder)();
            r.append(n), s.blob = r.getBlob("application/zip").size === 0;
          } catch {
            s.blob = !1;
          }
        }
      }
      try {
        s.nodestream = !!t("readable-stream").Readable;
      } catch {
        s.nodestream = !1;
      }
    }, { "readable-stream": 16 }], 31: [function(t, e, s) {
      for (var n = t("./utils"), r = t("./support"), o = t("./nodejsUtils"), a = t("./stream/GenericWorker"), l = new Array(256), h = 0; h < 256; h++)
        l[h] = 252 <= h ? 6 : 248 <= h ? 5 : 240 <= h ? 4 : 224 <= h ? 3 : 192 <= h ? 2 : 1;
      l[254] = l[254] = 1;
      function f() {
        a.call(this, "utf-8 decode"), this.leftOver = null;
      }
      function I() {
        a.call(this, "utf-8 encode");
      }
      s.utf8encode = function(u) {
        return r.nodebuffer ? o.newBufferFrom(u, "utf-8") : function(d) {
          var E, T, p, R, S, m = d.length, F = 0;
          for (R = 0; R < m; R++)
            (64512 & (T = d.charCodeAt(R))) == 55296 && R + 1 < m && (64512 & (p = d.charCodeAt(R + 1))) == 56320 && (T = 65536 + (T - 55296 << 10) + (p - 56320), R++), F += T < 128 ? 1 : T < 2048 ? 2 : T < 65536 ? 3 : 4;
          for (E = r.uint8array ? new Uint8Array(F) : new Array(F), R = S = 0; S < F; R++)
            (64512 & (T = d.charCodeAt(R))) == 55296 && R + 1 < m && (64512 & (p = d.charCodeAt(R + 1))) == 56320 && (T = 65536 + (T - 55296 << 10) + (p - 56320), R++), T < 128 ? E[S++] = T : (T < 2048 ? E[S++] = 192 | T >>> 6 : (T < 65536 ? E[S++] = 224 | T >>> 12 : (E[S++] = 240 | T >>> 18, E[S++] = 128 | T >>> 12 & 63), E[S++] = 128 | T >>> 6 & 63), E[S++] = 128 | 63 & T);
          return E;
        }(u);
      }, s.utf8decode = function(u) {
        return r.nodebuffer ? n.transformTo("nodebuffer", u).toString("utf-8") : function(d) {
          var E, T, p, R, S = d.length, m = new Array(2 * S);
          for (E = T = 0; E < S; )
            if ((p = d[E++]) < 128)
              m[T++] = p;
            else if (4 < (R = l[p]))
              m[T++] = 65533, E += R - 1;
            else {
              for (p &= R === 2 ? 31 : R === 3 ? 15 : 7; 1 < R && E < S; )
                p = p << 6 | 63 & d[E++], R--;
              1 < R ? m[T++] = 65533 : p < 65536 ? m[T++] = p : (p -= 65536, m[T++] = 55296 | p >> 10 & 1023, m[T++] = 56320 | 1023 & p);
            }
          return m.length !== T && (m.subarray ? m = m.subarray(0, T) : m.length = T), n.applyFromCharCode(m);
        }(u = n.transformTo(r.uint8array ? "uint8array" : "array", u));
      }, n.inherits(f, a), f.prototype.processChunk = function(u) {
        var d = n.transformTo(r.uint8array ? "uint8array" : "array", u.data);
        if (this.leftOver && this.leftOver.length) {
          if (r.uint8array) {
            var E = d;
            (d = new Uint8Array(E.length + this.leftOver.length)).set(this.leftOver, 0), d.set(E, this.leftOver.length);
          } else
            d = this.leftOver.concat(d);
          this.leftOver = null;
        }
        var T = function(R, S) {
          var m;
          for ((S = S || R.length) > R.length && (S = R.length), m = S - 1; 0 <= m && (192 & R[m]) == 128; )
            m--;
          return m < 0 || m === 0 ? S : m + l[R[m]] > S ? m : S;
        }(d), p = d;
        T !== d.length && (r.uint8array ? (p = d.subarray(0, T), this.leftOver = d.subarray(T, d.length)) : (p = d.slice(0, T), this.leftOver = d.slice(T, d.length))), this.push({ data: s.utf8decode(p), meta: u.meta });
      }, f.prototype.flush = function() {
        this.leftOver && this.leftOver.length && (this.push({ data: s.utf8decode(this.leftOver), meta: {} }), this.leftOver = null);
      }, s.Utf8DecodeWorker = f, n.inherits(I, a), I.prototype.processChunk = function(u) {
        this.push({ data: s.utf8encode(u.data), meta: u.meta });
      }, s.Utf8EncodeWorker = I;
    }, { "./nodejsUtils": 14, "./stream/GenericWorker": 28, "./support": 30, "./utils": 32 }], 32: [function(t, e, s) {
      var n = t("./support"), r = t("./base64"), o = t("./nodejsUtils"), a = t("./external");
      function l(E) {
        return E;
      }
      function h(E, T) {
        for (var p = 0; p < E.length; ++p)
          T[p] = 255 & E.charCodeAt(p);
        return T;
      }
      t("setimmediate"), s.newBlob = function(E, T) {
        s.checkSupport("blob");
        try {
          return new Blob([E], { type: T });
        } catch {
          try {
            var p = new (self.BlobBuilder || self.WebKitBlobBuilder || self.MozBlobBuilder || self.MSBlobBuilder)();
            return p.append(E), p.getBlob(T);
          } catch {
            throw new Error("Bug : can't construct the Blob.");
          }
        }
      };
      var f = { stringifyByChunk: function(E, T, p) {
        var R = [], S = 0, m = E.length;
        if (m <= p)
          return String.fromCharCode.apply(null, E);
        for (; S < m; )
          T === "array" || T === "nodebuffer" ? R.push(String.fromCharCode.apply(null, E.slice(S, Math.min(S + p, m)))) : R.push(String.fromCharCode.apply(null, E.subarray(S, Math.min(S + p, m)))), S += p;
        return R.join("");
      }, stringifyByChar: function(E) {
        for (var T = "", p = 0; p < E.length; p++)
          T += String.fromCharCode(E[p]);
        return T;
      }, applyCanBeUsed: { uint8array: function() {
        try {
          return n.uint8array && String.fromCharCode.apply(null, new Uint8Array(1)).length === 1;
        } catch {
          return !1;
        }
      }(), nodebuffer: function() {
        try {
          return n.nodebuffer && String.fromCharCode.apply(null, o.allocBuffer(1)).length === 1;
        } catch {
          return !1;
        }
      }() } };
      function I(E) {
        var T = 65536, p = s.getTypeOf(E), R = !0;
        if (p === "uint8array" ? R = f.applyCanBeUsed.uint8array : p === "nodebuffer" && (R = f.applyCanBeUsed.nodebuffer), R)
          for (; 1 < T; )
            try {
              return f.stringifyByChunk(E, p, T);
            } catch {
              T = Math.floor(T / 2);
            }
        return f.stringifyByChar(E);
      }
      function u(E, T) {
        for (var p = 0; p < E.length; p++)
          T[p] = E[p];
        return T;
      }
      s.applyFromCharCode = I;
      var d = {};
      d.string = { string: l, array: function(E) {
        return h(E, new Array(E.length));
      }, arraybuffer: function(E) {
        return d.string.uint8array(E).buffer;
      }, uint8array: function(E) {
        return h(E, new Uint8Array(E.length));
      }, nodebuffer: function(E) {
        return h(E, o.allocBuffer(E.length));
      } }, d.array = { string: I, array: l, arraybuffer: function(E) {
        return new Uint8Array(E).buffer;
      }, uint8array: function(E) {
        return new Uint8Array(E);
      }, nodebuffer: function(E) {
        return o.newBufferFrom(E);
      } }, d.arraybuffer = { string: function(E) {
        return I(new Uint8Array(E));
      }, array: function(E) {
        return u(new Uint8Array(E), new Array(E.byteLength));
      }, arraybuffer: l, uint8array: function(E) {
        return new Uint8Array(E);
      }, nodebuffer: function(E) {
        return o.newBufferFrom(new Uint8Array(E));
      } }, d.uint8array = { string: I, array: function(E) {
        return u(E, new Array(E.length));
      }, arraybuffer: function(E) {
        return E.buffer;
      }, uint8array: l, nodebuffer: function(E) {
        return o.newBufferFrom(E);
      } }, d.nodebuffer = { string: I, array: function(E) {
        return u(E, new Array(E.length));
      }, arraybuffer: function(E) {
        return d.nodebuffer.uint8array(E).buffer;
      }, uint8array: function(E) {
        return u(E, new Uint8Array(E.length));
      }, nodebuffer: l }, s.transformTo = function(E, T) {
        if (T = T || "", !E)
          return T;
        s.checkSupport(E);
        var p = s.getTypeOf(T);
        return d[p][E](T);
      }, s.resolve = function(E) {
        for (var T = E.split("/"), p = [], R = 0; R < T.length; R++) {
          var S = T[R];
          S === "." || S === "" && R !== 0 && R !== T.length - 1 || (S === ".." ? p.pop() : p.push(S));
        }
        return p.join("/");
      }, s.getTypeOf = function(E) {
        return typeof E == "string" ? "string" : Object.prototype.toString.call(E) === "[object Array]" ? "array" : n.nodebuffer && o.isBuffer(E) ? "nodebuffer" : n.uint8array && E instanceof Uint8Array ? "uint8array" : n.arraybuffer && E instanceof ArrayBuffer ? "arraybuffer" : void 0;
      }, s.checkSupport = function(E) {
        if (!n[E.toLowerCase()])
          throw new Error(E + " is not supported by this platform");
      }, s.MAX_VALUE_16BITS = 65535, s.MAX_VALUE_32BITS = -1, s.pretty = function(E) {
        var T, p, R = "";
        for (p = 0; p < (E || "").length; p++)
          R += "\\x" + ((T = E.charCodeAt(p)) < 16 ? "0" : "") + T.toString(16).toUpperCase();
        return R;
      }, s.delay = function(E, T, p) {
        setImmediate(function() {
          E.apply(p || null, T || []);
        });
      }, s.inherits = function(E, T) {
        function p() {
        }
        p.prototype = T.prototype, E.prototype = new p();
      }, s.extend = function() {
        var E, T, p = {};
        for (E = 0; E < arguments.length; E++)
          for (T in arguments[E])
            Object.prototype.hasOwnProperty.call(arguments[E], T) && p[T] === void 0 && (p[T] = arguments[E][T]);
        return p;
      }, s.prepareContent = function(E, T, p, R, S) {
        return a.Promise.resolve(T).then(function(m) {
          return n.blob && (m instanceof Blob || ["[object File]", "[object Blob]"].indexOf(Object.prototype.toString.call(m)) !== -1) && typeof FileReader < "u" ? new a.Promise(function(F, O) {
            var y = new FileReader();
            y.onload = function(w) {
              F(w.target.result);
            }, y.onerror = function(w) {
              O(w.target.error);
            }, y.readAsArrayBuffer(m);
          }) : m;
        }).then(function(m) {
          var F = s.getTypeOf(m);
          return F ? (F === "arraybuffer" ? m = s.transformTo("uint8array", m) : F === "string" && (S ? m = r.decode(m) : p && R !== !0 && (m = function(O) {
            return h(O, n.uint8array ? new Uint8Array(O.length) : new Array(O.length));
          }(m))), m) : a.Promise.reject(new Error("Can't read the data of '" + E + "'. Is it in a supported JavaScript type (String, Blob, ArrayBuffer, etc) ?"));
        });
      };
    }, { "./base64": 1, "./external": 6, "./nodejsUtils": 14, "./support": 30, setimmediate: 54 }], 33: [function(t, e, s) {
      var n = t("./reader/readerFor"), r = t("./utils"), o = t("./signature"), a = t("./zipEntry"), l = t("./support");
      function h(f) {
        this.files = [], this.loadOptions = f;
      }
      h.prototype = { checkSignature: function(f) {
        if (!this.reader.readAndCheckSignature(f)) {
          this.reader.index -= 4;
          var I = this.reader.readString(4);
          throw new Error("Corrupted zip or bug: unexpected signature (" + r.pretty(I) + ", expected " + r.pretty(f) + ")");
        }
      }, isSignature: function(f, I) {
        var u = this.reader.index;
        this.reader.setIndex(f);
        var d = this.reader.readString(4) === I;
        return this.reader.setIndex(u), d;
      }, readBlockEndOfCentral: function() {
        this.diskNumber = this.reader.readInt(2), this.diskWithCentralDirStart = this.reader.readInt(2), this.centralDirRecordsOnThisDisk = this.reader.readInt(2), this.centralDirRecords = this.reader.readInt(2), this.centralDirSize = this.reader.readInt(4), this.centralDirOffset = this.reader.readInt(4), this.zipCommentLength = this.reader.readInt(2);
        var f = this.reader.readData(this.zipCommentLength), I = l.uint8array ? "uint8array" : "array", u = r.transformTo(I, f);
        this.zipComment = this.loadOptions.decodeFileName(u);
      }, readBlockZip64EndOfCentral: function() {
        this.zip64EndOfCentralSize = this.reader.readInt(8), this.reader.skip(4), this.diskNumber = this.reader.readInt(4), this.diskWithCentralDirStart = this.reader.readInt(4), this.centralDirRecordsOnThisDisk = this.reader.readInt(8), this.centralDirRecords = this.reader.readInt(8), this.centralDirSize = this.reader.readInt(8), this.centralDirOffset = this.reader.readInt(8), this.zip64ExtensibleData = {};
        for (var f, I, u, d = this.zip64EndOfCentralSize - 44; 0 < d; )
          f = this.reader.readInt(2), I = this.reader.readInt(4), u = this.reader.readData(I), this.zip64ExtensibleData[f] = { id: f, length: I, value: u };
      }, readBlockZip64EndOfCentralLocator: function() {
        if (this.diskWithZip64CentralDirStart = this.reader.readInt(4), this.relativeOffsetEndOfZip64CentralDir = this.reader.readInt(8), this.disksCount = this.reader.readInt(4), 1 < this.disksCount)
          throw new Error("Multi-volumes zip are not supported");
      }, readLocalFiles: function() {
        var f, I;
        for (f = 0; f < this.files.length; f++)
          I = this.files[f], this.reader.setIndex(I.localHeaderOffset), this.checkSignature(o.LOCAL_FILE_HEADER), I.readLocalPart(this.reader), I.handleUTF8(), I.processAttributes();
      }, readCentralDir: function() {
        var f;
        for (this.reader.setIndex(this.centralDirOffset); this.reader.readAndCheckSignature(o.CENTRAL_FILE_HEADER); )
          (f = new a({ zip64: this.zip64 }, this.loadOptions)).readCentralPart(this.reader), this.files.push(f);
        if (this.centralDirRecords !== this.files.length && this.centralDirRecords !== 0 && this.files.length === 0)
          throw new Error("Corrupted zip or bug: expected " + this.centralDirRecords + " records in central dir, got " + this.files.length);
      }, readEndOfCentral: function() {
        var f = this.reader.lastIndexOfSignature(o.CENTRAL_DIRECTORY_END);
        if (f < 0)
          throw this.isSignature(0, o.LOCAL_FILE_HEADER) ? new Error("Corrupted zip: can't find end of central directory") : new Error("Can't find end of central directory : is this a zip file ? If it is, see https://stuk.github.io/jszip/documentation/howto/read_zip.html");
        this.reader.setIndex(f);
        var I = f;
        if (this.checkSignature(o.CENTRAL_DIRECTORY_END), this.readBlockEndOfCentral(), this.diskNumber === r.MAX_VALUE_16BITS || this.diskWithCentralDirStart === r.MAX_VALUE_16BITS || this.centralDirRecordsOnThisDisk === r.MAX_VALUE_16BITS || this.centralDirRecords === r.MAX_VALUE_16BITS || this.centralDirSize === r.MAX_VALUE_32BITS || this.centralDirOffset === r.MAX_VALUE_32BITS) {
          if (this.zip64 = !0, (f = this.reader.lastIndexOfSignature(o.ZIP64_CENTRAL_DIRECTORY_LOCATOR)) < 0)
            throw new Error("Corrupted zip: can't find the ZIP64 end of central directory locator");
          if (this.reader.setIndex(f), this.checkSignature(o.ZIP64_CENTRAL_DIRECTORY_LOCATOR), this.readBlockZip64EndOfCentralLocator(), !this.isSignature(this.relativeOffsetEndOfZip64CentralDir, o.ZIP64_CENTRAL_DIRECTORY_END) && (this.relativeOffsetEndOfZip64CentralDir = this.reader.lastIndexOfSignature(o.ZIP64_CENTRAL_DIRECTORY_END), this.relativeOffsetEndOfZip64CentralDir < 0))
            throw new Error("Corrupted zip: can't find the ZIP64 end of central directory");
          this.reader.setIndex(this.relativeOffsetEndOfZip64CentralDir), this.checkSignature(o.ZIP64_CENTRAL_DIRECTORY_END), this.readBlockZip64EndOfCentral();
        }
        var u = this.centralDirOffset + this.centralDirSize;
        this.zip64 && (u += 20, u += 12 + this.zip64EndOfCentralSize);
        var d = I - u;
        if (0 < d)
          this.isSignature(I, o.CENTRAL_FILE_HEADER) || (this.reader.zero = d);
        else if (d < 0)
          throw new Error("Corrupted zip: missing " + Math.abs(d) + " bytes.");
      }, prepareReader: function(f) {
        this.reader = n(f);
      }, load: function(f) {
        this.prepareReader(f), this.readEndOfCentral(), this.readCentralDir(), this.readLocalFiles();
      } }, e.exports = h;
    }, { "./reader/readerFor": 22, "./signature": 23, "./support": 30, "./utils": 32, "./zipEntry": 34 }], 34: [function(t, e, s) {
      var n = t("./reader/readerFor"), r = t("./utils"), o = t("./compressedObject"), a = t("./crc32"), l = t("./utf8"), h = t("./compressions"), f = t("./support");
      function I(u, d) {
        this.options = u, this.loadOptions = d;
      }
      I.prototype = { isEncrypted: function() {
        return (1 & this.bitFlag) == 1;
      }, useUTF8: function() {
        return (2048 & this.bitFlag) == 2048;
      }, readLocalPart: function(u) {
        var d, E;
        if (u.skip(22), this.fileNameLength = u.readInt(2), E = u.readInt(2), this.fileName = u.readData(this.fileNameLength), u.skip(E), this.compressedSize === -1 || this.uncompressedSize === -1)
          throw new Error("Bug or corrupted zip : didn't get enough information from the central directory (compressedSize === -1 || uncompressedSize === -1)");
        if ((d = function(T) {
          for (var p in h)
            if (Object.prototype.hasOwnProperty.call(h, p) && h[p].magic === T)
              return h[p];
          return null;
        }(this.compressionMethod)) === null)
          throw new Error("Corrupted zip : compression " + r.pretty(this.compressionMethod) + " unknown (inner file : " + r.transformTo("string", this.fileName) + ")");
        this.decompressed = new o(this.compressedSize, this.uncompressedSize, this.crc32, d, u.readData(this.compressedSize));
      }, readCentralPart: function(u) {
        this.versionMadeBy = u.readInt(2), u.skip(2), this.bitFlag = u.readInt(2), this.compressionMethod = u.readString(2), this.date = u.readDate(), this.crc32 = u.readInt(4), this.compressedSize = u.readInt(4), this.uncompressedSize = u.readInt(4);
        var d = u.readInt(2);
        if (this.extraFieldsLength = u.readInt(2), this.fileCommentLength = u.readInt(2), this.diskNumberStart = u.readInt(2), this.internalFileAttributes = u.readInt(2), this.externalFileAttributes = u.readInt(4), this.localHeaderOffset = u.readInt(4), this.isEncrypted())
          throw new Error("Encrypted zip are not supported");
        u.skip(d), this.readExtraFields(u), this.parseZIP64ExtraField(u), this.fileComment = u.readData(this.fileCommentLength);
      }, processAttributes: function() {
        this.unixPermissions = null, this.dosPermissions = null;
        var u = this.versionMadeBy >> 8;
        this.dir = !!(16 & this.externalFileAttributes), u == 0 && (this.dosPermissions = 63 & this.externalFileAttributes), u == 3 && (this.unixPermissions = this.externalFileAttributes >> 16 & 65535), this.dir || this.fileNameStr.slice(-1) !== "/" || (this.dir = !0);
      }, parseZIP64ExtraField: function() {
        if (this.extraFields[1]) {
          var u = n(this.extraFields[1].value);
          this.uncompressedSize === r.MAX_VALUE_32BITS && (this.uncompressedSize = u.readInt(8)), this.compressedSize === r.MAX_VALUE_32BITS && (this.compressedSize = u.readInt(8)), this.localHeaderOffset === r.MAX_VALUE_32BITS && (this.localHeaderOffset = u.readInt(8)), this.diskNumberStart === r.MAX_VALUE_32BITS && (this.diskNumberStart = u.readInt(4));
        }
      }, readExtraFields: function(u) {
        var d, E, T, p = u.index + this.extraFieldsLength;
        for (this.extraFields || (this.extraFields = {}); u.index + 4 < p; )
          d = u.readInt(2), E = u.readInt(2), T = u.readData(E), this.extraFields[d] = { id: d, length: E, value: T };
        u.setIndex(p);
      }, handleUTF8: function() {
        var u = f.uint8array ? "uint8array" : "array";
        if (this.useUTF8())
          this.fileNameStr = l.utf8decode(this.fileName), this.fileCommentStr = l.utf8decode(this.fileComment);
        else {
          var d = this.findExtraFieldUnicodePath();
          if (d !== null)
            this.fileNameStr = d;
          else {
            var E = r.transformTo(u, this.fileName);
            this.fileNameStr = this.loadOptions.decodeFileName(E);
          }
          var T = this.findExtraFieldUnicodeComment();
          if (T !== null)
            this.fileCommentStr = T;
          else {
            var p = r.transformTo(u, this.fileComment);
            this.fileCommentStr = this.loadOptions.decodeFileName(p);
          }
        }
      }, findExtraFieldUnicodePath: function() {
        var u = this.extraFields[28789];
        if (u) {
          var d = n(u.value);
          return d.readInt(1) !== 1 || a(this.fileName) !== d.readInt(4) ? null : l.utf8decode(d.readData(u.length - 5));
        }
        return null;
      }, findExtraFieldUnicodeComment: function() {
        var u = this.extraFields[25461];
        if (u) {
          var d = n(u.value);
          return d.readInt(1) !== 1 || a(this.fileComment) !== d.readInt(4) ? null : l.utf8decode(d.readData(u.length - 5));
        }
        return null;
      } }, e.exports = I;
    }, { "./compressedObject": 2, "./compressions": 3, "./crc32": 4, "./reader/readerFor": 22, "./support": 30, "./utf8": 31, "./utils": 32 }], 35: [function(t, e, s) {
      function n(d, E, T) {
        this.name = d, this.dir = T.dir, this.date = T.date, this.comment = T.comment, this.unixPermissions = T.unixPermissions, this.dosPermissions = T.dosPermissions, this._data = E, this._dataBinary = T.binary, this.options = { compression: T.compression, compressionOptions: T.compressionOptions };
      }
      var r = t("./stream/StreamHelper"), o = t("./stream/DataWorker"), a = t("./utf8"), l = t("./compressedObject"), h = t("./stream/GenericWorker");
      n.prototype = { internalStream: function(d) {
        var E = null, T = "string";
        try {
          if (!d)
            throw new Error("No output type specified.");
          var p = (T = d.toLowerCase()) === "string" || T === "text";
          T !== "binarystring" && T !== "text" || (T = "string"), E = this._decompressWorker();
          var R = !this._dataBinary;
          R && !p && (E = E.pipe(new a.Utf8EncodeWorker())), !R && p && (E = E.pipe(new a.Utf8DecodeWorker()));
        } catch (S) {
          (E = new h("error")).error(S);
        }
        return new r(E, T, "");
      }, async: function(d, E) {
        return this.internalStream(d).accumulate(E);
      }, nodeStream: function(d, E) {
        return this.internalStream(d || "nodebuffer").toNodejsStream(E);
      }, _compressWorker: function(d, E) {
        if (this._data instanceof l && this._data.compression.magic === d.magic)
          return this._data.getCompressedWorker();
        var T = this._decompressWorker();
        return this._dataBinary || (T = T.pipe(new a.Utf8EncodeWorker())), l.createWorkerFrom(T, d, E);
      }, _decompressWorker: function() {
        return this._data instanceof l ? this._data.getContentWorker() : this._data instanceof h ? this._data : new o(this._data);
      } };
      for (var f = ["asText", "asBinary", "asNodeBuffer", "asUint8Array", "asArrayBuffer"], I = function() {
        throw new Error("This method has been removed in JSZip 3.0, please check the upgrade guide.");
      }, u = 0; u < f.length; u++)
        n.prototype[f[u]] = I;
      e.exports = n;
    }, { "./compressedObject": 2, "./stream/DataWorker": 27, "./stream/GenericWorker": 28, "./stream/StreamHelper": 29, "./utf8": 31 }], 36: [function(t, e, s) {
      (function(n) {
        var r, o, a = n.MutationObserver || n.WebKitMutationObserver;
        if (a) {
          var l = 0, h = new a(d), f = n.document.createTextNode("");
          h.observe(f, { characterData: !0 }), r = function() {
            f.data = l = ++l % 2;
          };
        } else if (n.setImmediate || n.MessageChannel === void 0)
          r = "document" in n && "onreadystatechange" in n.document.createElement("script") ? function() {
            var E = n.document.createElement("script");
            E.onreadystatechange = function() {
              d(), E.onreadystatechange = null, E.parentNode.removeChild(E), E = null;
            }, n.document.documentElement.appendChild(E);
          } : function() {
            setTimeout(d, 0);
          };
        else {
          var I = new n.MessageChannel();
          I.port1.onmessage = d, r = function() {
            I.port2.postMessage(0);
          };
        }
        var u = [];
        function d() {
          var E, T;
          o = !0;
          for (var p = u.length; p; ) {
            for (T = u, u = [], E = -1; ++E < p; )
              T[E]();
            p = u.length;
          }
          o = !1;
        }
        e.exports = function(E) {
          u.push(E) !== 1 || o || r();
        };
      }).call(this, typeof us < "u" ? us : typeof self < "u" ? self : typeof window < "u" ? window : {});
    }, {}], 37: [function(t, e, s) {
      var n = t("immediate");
      function r() {
      }
      var o = {}, a = ["REJECTED"], l = ["FULFILLED"], h = ["PENDING"];
      function f(p) {
        if (typeof p != "function")
          throw new TypeError("resolver must be a function");
        this.state = h, this.queue = [], this.outcome = void 0, p !== r && E(this, p);
      }
      function I(p, R, S) {
        this.promise = p, typeof R == "function" && (this.onFulfilled = R, this.callFulfilled = this.otherCallFulfilled), typeof S == "function" && (this.onRejected = S, this.callRejected = this.otherCallRejected);
      }
      function u(p, R, S) {
        n(function() {
          var m;
          try {
            m = R(S);
          } catch (F) {
            return o.reject(p, F);
          }
          m === p ? o.reject(p, new TypeError("Cannot resolve promise with itself")) : o.resolve(p, m);
        });
      }
      function d(p) {
        var R = p && p.then;
        if (p && (typeof p == "object" || typeof p == "function") && typeof R == "function")
          return function() {
            R.apply(p, arguments);
          };
      }
      function E(p, R) {
        var S = !1;
        function m(y) {
          S || (S = !0, o.reject(p, y));
        }
        function F(y) {
          S || (S = !0, o.resolve(p, y));
        }
        var O = T(function() {
          R(F, m);
        });
        O.status === "error" && m(O.value);
      }
      function T(p, R) {
        var S = {};
        try {
          S.value = p(R), S.status = "success";
        } catch (m) {
          S.status = "error", S.value = m;
        }
        return S;
      }
      (e.exports = f).prototype.finally = function(p) {
        if (typeof p != "function")
          return this;
        var R = this.constructor;
        return this.then(function(S) {
          return R.resolve(p()).then(function() {
            return S;
          });
        }, function(S) {
          return R.resolve(p()).then(function() {
            throw S;
          });
        });
      }, f.prototype.catch = function(p) {
        return this.then(null, p);
      }, f.prototype.then = function(p, R) {
        if (typeof p != "function" && this.state === l || typeof R != "function" && this.state === a)
          return this;
        var S = new this.constructor(r);
        return this.state !== h ? u(S, this.state === l ? p : R, this.outcome) : this.queue.push(new I(S, p, R)), S;
      }, I.prototype.callFulfilled = function(p) {
        o.resolve(this.promise, p);
      }, I.prototype.otherCallFulfilled = function(p) {
        u(this.promise, this.onFulfilled, p);
      }, I.prototype.callRejected = function(p) {
        o.reject(this.promise, p);
      }, I.prototype.otherCallRejected = function(p) {
        u(this.promise, this.onRejected, p);
      }, o.resolve = function(p, R) {
        var S = T(d, R);
        if (S.status === "error")
          return o.reject(p, S.value);
        var m = S.value;
        if (m)
          E(p, m);
        else {
          p.state = l, p.outcome = R;
          for (var F = -1, O = p.queue.length; ++F < O; )
            p.queue[F].callFulfilled(R);
        }
        return p;
      }, o.reject = function(p, R) {
        p.state = a, p.outcome = R;
        for (var S = -1, m = p.queue.length; ++S < m; )
          p.queue[S].callRejected(R);
        return p;
      }, f.resolve = function(p) {
        return p instanceof this ? p : o.resolve(new this(r), p);
      }, f.reject = function(p) {
        var R = new this(r);
        return o.reject(R, p);
      }, f.all = function(p) {
        var R = this;
        if (Object.prototype.toString.call(p) !== "[object Array]")
          return this.reject(new TypeError("must be an array"));
        var S = p.length, m = !1;
        if (!S)
          return this.resolve([]);
        for (var F = new Array(S), O = 0, y = -1, w = new this(r); ++y < S; )
          L(p[y], y);
        return w;
        function L(b, Y) {
          R.resolve(b).then(function(N) {
            F[Y] = N, ++O !== S || m || (m = !0, o.resolve(w, F));
          }, function(N) {
            m || (m = !0, o.reject(w, N));
          });
        }
      }, f.race = function(p) {
        var R = this;
        if (Object.prototype.toString.call(p) !== "[object Array]")
          return this.reject(new TypeError("must be an array"));
        var S = p.length, m = !1;
        if (!S)
          return this.resolve([]);
        for (var F = -1, O = new this(r); ++F < S; )
          y = p[F], R.resolve(y).then(function(w) {
            m || (m = !0, o.resolve(O, w));
          }, function(w) {
            m || (m = !0, o.reject(O, w));
          });
        var y;
        return O;
      };
    }, { immediate: 36 }], 38: [function(t, e, s) {
      var n = {};
      (0, t("./lib/utils/common").assign)(n, t("./lib/deflate"), t("./lib/inflate"), t("./lib/zlib/constants")), e.exports = n;
    }, { "./lib/deflate": 39, "./lib/inflate": 40, "./lib/utils/common": 41, "./lib/zlib/constants": 44 }], 39: [function(t, e, s) {
      var n = t("./zlib/deflate"), r = t("./utils/common"), o = t("./utils/strings"), a = t("./zlib/messages"), l = t("./zlib/zstream"), h = Object.prototype.toString, f = 0, I = -1, u = 0, d = 8;
      function E(p) {
        if (!(this instanceof E))
          return new E(p);
        this.options = r.assign({ level: I, method: d, chunkSize: 16384, windowBits: 15, memLevel: 8, strategy: u, to: "" }, p || {});
        var R = this.options;
        R.raw && 0 < R.windowBits ? R.windowBits = -R.windowBits : R.gzip && 0 < R.windowBits && R.windowBits < 16 && (R.windowBits += 16), this.err = 0, this.msg = "", this.ended = !1, this.chunks = [], this.strm = new l(), this.strm.avail_out = 0;
        var S = n.deflateInit2(this.strm, R.level, R.method, R.windowBits, R.memLevel, R.strategy);
        if (S !== f)
          throw new Error(a[S]);
        if (R.header && n.deflateSetHeader(this.strm, R.header), R.dictionary) {
          var m;
          if (m = typeof R.dictionary == "string" ? o.string2buf(R.dictionary) : h.call(R.dictionary) === "[object ArrayBuffer]" ? new Uint8Array(R.dictionary) : R.dictionary, (S = n.deflateSetDictionary(this.strm, m)) !== f)
            throw new Error(a[S]);
          this._dict_set = !0;
        }
      }
      function T(p, R) {
        var S = new E(R);
        if (S.push(p, !0), S.err)
          throw S.msg || a[S.err];
        return S.result;
      }
      E.prototype.push = function(p, R) {
        var S, m, F = this.strm, O = this.options.chunkSize;
        if (this.ended)
          return !1;
        m = R === ~~R ? R : R === !0 ? 4 : 0, typeof p == "string" ? F.input = o.string2buf(p) : h.call(p) === "[object ArrayBuffer]" ? F.input = new Uint8Array(p) : F.input = p, F.next_in = 0, F.avail_in = F.input.length;
        do {
          if (F.avail_out === 0 && (F.output = new r.Buf8(O), F.next_out = 0, F.avail_out = O), (S = n.deflate(F, m)) !== 1 && S !== f)
            return this.onEnd(S), !(this.ended = !0);
          F.avail_out !== 0 && (F.avail_in !== 0 || m !== 4 && m !== 2) || (this.options.to === "string" ? this.onData(o.buf2binstring(r.shrinkBuf(F.output, F.next_out))) : this.onData(r.shrinkBuf(F.output, F.next_out)));
        } while ((0 < F.avail_in || F.avail_out === 0) && S !== 1);
        return m === 4 ? (S = n.deflateEnd(this.strm), this.onEnd(S), this.ended = !0, S === f) : m !== 2 || (this.onEnd(f), !(F.avail_out = 0));
      }, E.prototype.onData = function(p) {
        this.chunks.push(p);
      }, E.prototype.onEnd = function(p) {
        p === f && (this.options.to === "string" ? this.result = this.chunks.join("") : this.result = r.flattenChunks(this.chunks)), this.chunks = [], this.err = p, this.msg = this.strm.msg;
      }, s.Deflate = E, s.deflate = T, s.deflateRaw = function(p, R) {
        return (R = R || {}).raw = !0, T(p, R);
      }, s.gzip = function(p, R) {
        return (R = R || {}).gzip = !0, T(p, R);
      };
    }, { "./utils/common": 41, "./utils/strings": 42, "./zlib/deflate": 46, "./zlib/messages": 51, "./zlib/zstream": 53 }], 40: [function(t, e, s) {
      var n = t("./zlib/inflate"), r = t("./utils/common"), o = t("./utils/strings"), a = t("./zlib/constants"), l = t("./zlib/messages"), h = t("./zlib/zstream"), f = t("./zlib/gzheader"), I = Object.prototype.toString;
      function u(E) {
        if (!(this instanceof u))
          return new u(E);
        this.options = r.assign({ chunkSize: 16384, windowBits: 0, to: "" }, E || {});
        var T = this.options;
        T.raw && 0 <= T.windowBits && T.windowBits < 16 && (T.windowBits = -T.windowBits, T.windowBits === 0 && (T.windowBits = -15)), !(0 <= T.windowBits && T.windowBits < 16) || E && E.windowBits || (T.windowBits += 32), 15 < T.windowBits && T.windowBits < 48 && !(15 & T.windowBits) && (T.windowBits |= 15), this.err = 0, this.msg = "", this.ended = !1, this.chunks = [], this.strm = new h(), this.strm.avail_out = 0;
        var p = n.inflateInit2(this.strm, T.windowBits);
        if (p !== a.Z_OK)
          throw new Error(l[p]);
        this.header = new f(), n.inflateGetHeader(this.strm, this.header);
      }
      function d(E, T) {
        var p = new u(T);
        if (p.push(E, !0), p.err)
          throw p.msg || l[p.err];
        return p.result;
      }
      u.prototype.push = function(E, T) {
        var p, R, S, m, F, O, y = this.strm, w = this.options.chunkSize, L = this.options.dictionary, b = !1;
        if (this.ended)
          return !1;
        R = T === ~~T ? T : T === !0 ? a.Z_FINISH : a.Z_NO_FLUSH, typeof E == "string" ? y.input = o.binstring2buf(E) : I.call(E) === "[object ArrayBuffer]" ? y.input = new Uint8Array(E) : y.input = E, y.next_in = 0, y.avail_in = y.input.length;
        do {
          if (y.avail_out === 0 && (y.output = new r.Buf8(w), y.next_out = 0, y.avail_out = w), (p = n.inflate(y, a.Z_NO_FLUSH)) === a.Z_NEED_DICT && L && (O = typeof L == "string" ? o.string2buf(L) : I.call(L) === "[object ArrayBuffer]" ? new Uint8Array(L) : L, p = n.inflateSetDictionary(this.strm, O)), p === a.Z_BUF_ERROR && b === !0 && (p = a.Z_OK, b = !1), p !== a.Z_STREAM_END && p !== a.Z_OK)
            return this.onEnd(p), !(this.ended = !0);
          y.next_out && (y.avail_out !== 0 && p !== a.Z_STREAM_END && (y.avail_in !== 0 || R !== a.Z_FINISH && R !== a.Z_SYNC_FLUSH) || (this.options.to === "string" ? (S = o.utf8border(y.output, y.next_out), m = y.next_out - S, F = o.buf2string(y.output, S), y.next_out = m, y.avail_out = w - m, m && r.arraySet(y.output, y.output, S, m, 0), this.onData(F)) : this.onData(r.shrinkBuf(y.output, y.next_out)))), y.avail_in === 0 && y.avail_out === 0 && (b = !0);
        } while ((0 < y.avail_in || y.avail_out === 0) && p !== a.Z_STREAM_END);
        return p === a.Z_STREAM_END && (R = a.Z_FINISH), R === a.Z_FINISH ? (p = n.inflateEnd(this.strm), this.onEnd(p), this.ended = !0, p === a.Z_OK) : R !== a.Z_SYNC_FLUSH || (this.onEnd(a.Z_OK), !(y.avail_out = 0));
      }, u.prototype.onData = function(E) {
        this.chunks.push(E);
      }, u.prototype.onEnd = function(E) {
        E === a.Z_OK && (this.options.to === "string" ? this.result = this.chunks.join("") : this.result = r.flattenChunks(this.chunks)), this.chunks = [], this.err = E, this.msg = this.strm.msg;
      }, s.Inflate = u, s.inflate = d, s.inflateRaw = function(E, T) {
        return (T = T || {}).raw = !0, d(E, T);
      }, s.ungzip = d;
    }, { "./utils/common": 41, "./utils/strings": 42, "./zlib/constants": 44, "./zlib/gzheader": 47, "./zlib/inflate": 49, "./zlib/messages": 51, "./zlib/zstream": 53 }], 41: [function(t, e, s) {
      var n = typeof Uint8Array < "u" && typeof Uint16Array < "u" && typeof Int32Array < "u";
      s.assign = function(a) {
        for (var l = Array.prototype.slice.call(arguments, 1); l.length; ) {
          var h = l.shift();
          if (h) {
            if (typeof h != "object")
              throw new TypeError(h + "must be non-object");
            for (var f in h)
              h.hasOwnProperty(f) && (a[f] = h[f]);
          }
        }
        return a;
      }, s.shrinkBuf = function(a, l) {
        return a.length === l ? a : a.subarray ? a.subarray(0, l) : (a.length = l, a);
      };
      var r = { arraySet: function(a, l, h, f, I) {
        if (l.subarray && a.subarray)
          a.set(l.subarray(h, h + f), I);
        else
          for (var u = 0; u < f; u++)
            a[I + u] = l[h + u];
      }, flattenChunks: function(a) {
        var l, h, f, I, u, d;
        for (l = f = 0, h = a.length; l < h; l++)
          f += a[l].length;
        for (d = new Uint8Array(f), l = I = 0, h = a.length; l < h; l++)
          u = a[l], d.set(u, I), I += u.length;
        return d;
      } }, o = { arraySet: function(a, l, h, f, I) {
        for (var u = 0; u < f; u++)
          a[I + u] = l[h + u];
      }, flattenChunks: function(a) {
        return [].concat.apply([], a);
      } };
      s.setTyped = function(a) {
        a ? (s.Buf8 = Uint8Array, s.Buf16 = Uint16Array, s.Buf32 = Int32Array, s.assign(s, r)) : (s.Buf8 = Array, s.Buf16 = Array, s.Buf32 = Array, s.assign(s, o));
      }, s.setTyped(n);
    }, {}], 42: [function(t, e, s) {
      var n = t("./common"), r = !0, o = !0;
      try {
        String.fromCharCode.apply(null, [0]);
      } catch {
        r = !1;
      }
      try {
        String.fromCharCode.apply(null, new Uint8Array(1));
      } catch {
        o = !1;
      }
      for (var a = new n.Buf8(256), l = 0; l < 256; l++)
        a[l] = 252 <= l ? 6 : 248 <= l ? 5 : 240 <= l ? 4 : 224 <= l ? 3 : 192 <= l ? 2 : 1;
      function h(f, I) {
        if (I < 65537 && (f.subarray && o || !f.subarray && r))
          return String.fromCharCode.apply(null, n.shrinkBuf(f, I));
        for (var u = "", d = 0; d < I; d++)
          u += String.fromCharCode(f[d]);
        return u;
      }
      a[254] = a[254] = 1, s.string2buf = function(f) {
        var I, u, d, E, T, p = f.length, R = 0;
        for (E = 0; E < p; E++)
          (64512 & (u = f.charCodeAt(E))) == 55296 && E + 1 < p && (64512 & (d = f.charCodeAt(E + 1))) == 56320 && (u = 65536 + (u - 55296 << 10) + (d - 56320), E++), R += u < 128 ? 1 : u < 2048 ? 2 : u < 65536 ? 3 : 4;
        for (I = new n.Buf8(R), E = T = 0; T < R; E++)
          (64512 & (u = f.charCodeAt(E))) == 55296 && E + 1 < p && (64512 & (d = f.charCodeAt(E + 1))) == 56320 && (u = 65536 + (u - 55296 << 10) + (d - 56320), E++), u < 128 ? I[T++] = u : (u < 2048 ? I[T++] = 192 | u >>> 6 : (u < 65536 ? I[T++] = 224 | u >>> 12 : (I[T++] = 240 | u >>> 18, I[T++] = 128 | u >>> 12 & 63), I[T++] = 128 | u >>> 6 & 63), I[T++] = 128 | 63 & u);
        return I;
      }, s.buf2binstring = function(f) {
        return h(f, f.length);
      }, s.binstring2buf = function(f) {
        for (var I = new n.Buf8(f.length), u = 0, d = I.length; u < d; u++)
          I[u] = f.charCodeAt(u);
        return I;
      }, s.buf2string = function(f, I) {
        var u, d, E, T, p = I || f.length, R = new Array(2 * p);
        for (u = d = 0; u < p; )
          if ((E = f[u++]) < 128)
            R[d++] = E;
          else if (4 < (T = a[E]))
            R[d++] = 65533, u += T - 1;
          else {
            for (E &= T === 2 ? 31 : T === 3 ? 15 : 7; 1 < T && u < p; )
              E = E << 6 | 63 & f[u++], T--;
            1 < T ? R[d++] = 65533 : E < 65536 ? R[d++] = E : (E -= 65536, R[d++] = 55296 | E >> 10 & 1023, R[d++] = 56320 | 1023 & E);
          }
        return h(R, d);
      }, s.utf8border = function(f, I) {
        var u;
        for ((I = I || f.length) > f.length && (I = f.length), u = I - 1; 0 <= u && (192 & f[u]) == 128; )
          u--;
        return u < 0 || u === 0 ? I : u + a[f[u]] > I ? u : I;
      };
    }, { "./common": 41 }], 43: [function(t, e, s) {
      e.exports = function(n, r, o, a) {
        for (var l = 65535 & n | 0, h = n >>> 16 & 65535 | 0, f = 0; o !== 0; ) {
          for (o -= f = 2e3 < o ? 2e3 : o; h = h + (l = l + r[a++] | 0) | 0, --f; )
            ;
          l %= 65521, h %= 65521;
        }
        return l | h << 16 | 0;
      };
    }, {}], 44: [function(t, e, s) {
      e.exports = { Z_NO_FLUSH: 0, Z_PARTIAL_FLUSH: 1, Z_SYNC_FLUSH: 2, Z_FULL_FLUSH: 3, Z_FINISH: 4, Z_BLOCK: 5, Z_TREES: 6, Z_OK: 0, Z_STREAM_END: 1, Z_NEED_DICT: 2, Z_ERRNO: -1, Z_STREAM_ERROR: -2, Z_DATA_ERROR: -3, Z_BUF_ERROR: -5, Z_NO_COMPRESSION: 0, Z_BEST_SPEED: 1, Z_BEST_COMPRESSION: 9, Z_DEFAULT_COMPRESSION: -1, Z_FILTERED: 1, Z_HUFFMAN_ONLY: 2, Z_RLE: 3, Z_FIXED: 4, Z_DEFAULT_STRATEGY: 0, Z_BINARY: 0, Z_TEXT: 1, Z_UNKNOWN: 2, Z_DEFLATED: 8 };
    }, {}], 45: [function(t, e, s) {
      var n = function() {
        for (var r, o = [], a = 0; a < 256; a++) {
          r = a;
          for (var l = 0; l < 8; l++)
            r = 1 & r ? 3988292384 ^ r >>> 1 : r >>> 1;
          o[a] = r;
        }
        return o;
      }();
      e.exports = function(r, o, a, l) {
        var h = n, f = l + a;
        r ^= -1;
        for (var I = l; I < f; I++)
          r = r >>> 8 ^ h[255 & (r ^ o[I])];
        return -1 ^ r;
      };
    }, {}], 46: [function(t, e, s) {
      var n, r = t("../utils/common"), o = t("./trees"), a = t("./adler32"), l = t("./crc32"), h = t("./messages"), f = 0, I = 4, u = 0, d = -2, E = -1, T = 4, p = 2, R = 8, S = 9, m = 286, F = 30, O = 19, y = 2 * m + 1, w = 15, L = 3, b = 258, Y = b + L + 1, N = 42, M = 113, g = 1, v = 2, q = 3, G = 4;
      function et(A, X) {
        return A.msg = h[X], X;
      }
      function W(A) {
        return (A << 1) - (4 < A ? 9 : 0);
      }
      function nt(A) {
        for (var X = A.length; 0 <= --X; )
          A[X] = 0;
      }
      function V(A) {
        var X = A.state, z = X.pending;
        z > A.avail_out && (z = A.avail_out), z !== 0 && (r.arraySet(A.output, X.pending_buf, X.pending_out, z, A.next_out), A.next_out += z, X.pending_out += z, A.total_out += z, A.avail_out -= z, X.pending -= z, X.pending === 0 && (X.pending_out = 0));
      }
      function x(A, X) {
        o._tr_flush_block(A, 0 <= A.block_start ? A.block_start : -1, A.strstart - A.block_start, X), A.block_start = A.strstart, V(A.strm);
      }
      function ot(A, X) {
        A.pending_buf[A.pending++] = X;
      }
      function it(A, X) {
        A.pending_buf[A.pending++] = X >>> 8 & 255, A.pending_buf[A.pending++] = 255 & X;
      }
      function tt(A, X) {
        var z, _, P = A.max_chain_length, U = A.strstart, $ = A.prev_length, Z = A.nice_match, B = A.strstart > A.w_size - Y ? A.strstart - (A.w_size - Y) : 0, K = A.window, st = A.w_mask, J = A.prev, ct = A.strstart + b, Rt = K[U + $ - 1], dt = K[U + $];
        A.prev_length >= A.good_match && (P >>= 2), Z > A.lookahead && (Z = A.lookahead);
        do
          if (K[(z = X) + $] === dt && K[z + $ - 1] === Rt && K[z] === K[U] && K[++z] === K[U + 1]) {
            U += 2, z++;
            do
              ;
            while (K[++U] === K[++z] && K[++U] === K[++z] && K[++U] === K[++z] && K[++U] === K[++z] && K[++U] === K[++z] && K[++U] === K[++z] && K[++U] === K[++z] && K[++U] === K[++z] && U < ct);
            if (_ = b - (ct - U), U = ct - b, $ < _) {
              if (A.match_start = X, Z <= ($ = _))
                break;
              Rt = K[U + $ - 1], dt = K[U + $];
            }
          }
        while ((X = J[X & st]) > B && --P != 0);
        return $ <= A.lookahead ? $ : A.lookahead;
      }
      function St(A) {
        var X, z, _, P, U, $, Z, B, K, st, J = A.w_size;
        do {
          if (P = A.window_size - A.lookahead - A.strstart, A.strstart >= J + (J - Y)) {
            for (r.arraySet(A.window, A.window, J, J, 0), A.match_start -= J, A.strstart -= J, A.block_start -= J, X = z = A.hash_size; _ = A.head[--X], A.head[X] = J <= _ ? _ - J : 0, --z; )
              ;
            for (X = z = J; _ = A.prev[--X], A.prev[X] = J <= _ ? _ - J : 0, --z; )
              ;
            P += J;
          }
          if (A.strm.avail_in === 0)
            break;
          if ($ = A.strm, Z = A.window, B = A.strstart + A.lookahead, K = P, st = void 0, st = $.avail_in, K < st && (st = K), z = st === 0 ? 0 : ($.avail_in -= st, r.arraySet(Z, $.input, $.next_in, st, B), $.state.wrap === 1 ? $.adler = a($.adler, Z, st, B) : $.state.wrap === 2 && ($.adler = l($.adler, Z, st, B)), $.next_in += st, $.total_in += st, st), A.lookahead += z, A.lookahead + A.insert >= L)
            for (U = A.strstart - A.insert, A.ins_h = A.window[U], A.ins_h = (A.ins_h << A.hash_shift ^ A.window[U + 1]) & A.hash_mask; A.insert && (A.ins_h = (A.ins_h << A.hash_shift ^ A.window[U + L - 1]) & A.hash_mask, A.prev[U & A.w_mask] = A.head[A.ins_h], A.head[A.ins_h] = U, U++, A.insert--, !(A.lookahead + A.insert < L)); )
              ;
        } while (A.lookahead < Y && A.strm.avail_in !== 0);
      }
      function yt(A, X) {
        for (var z, _; ; ) {
          if (A.lookahead < Y) {
            if (St(A), A.lookahead < Y && X === f)
              return g;
            if (A.lookahead === 0)
              break;
          }
          if (z = 0, A.lookahead >= L && (A.ins_h = (A.ins_h << A.hash_shift ^ A.window[A.strstart + L - 1]) & A.hash_mask, z = A.prev[A.strstart & A.w_mask] = A.head[A.ins_h], A.head[A.ins_h] = A.strstart), z !== 0 && A.strstart - z <= A.w_size - Y && (A.match_length = tt(A, z)), A.match_length >= L)
            if (_ = o._tr_tally(A, A.strstart - A.match_start, A.match_length - L), A.lookahead -= A.match_length, A.match_length <= A.max_lazy_match && A.lookahead >= L) {
              for (A.match_length--; A.strstart++, A.ins_h = (A.ins_h << A.hash_shift ^ A.window[A.strstart + L - 1]) & A.hash_mask, z = A.prev[A.strstart & A.w_mask] = A.head[A.ins_h], A.head[A.ins_h] = A.strstart, --A.match_length != 0; )
                ;
              A.strstart++;
            } else
              A.strstart += A.match_length, A.match_length = 0, A.ins_h = A.window[A.strstart], A.ins_h = (A.ins_h << A.hash_shift ^ A.window[A.strstart + 1]) & A.hash_mask;
          else
            _ = o._tr_tally(A, 0, A.window[A.strstart]), A.lookahead--, A.strstart++;
          if (_ && (x(A, !1), A.strm.avail_out === 0))
            return g;
        }
        return A.insert = A.strstart < L - 1 ? A.strstart : L - 1, X === I ? (x(A, !0), A.strm.avail_out === 0 ? q : G) : A.last_lit && (x(A, !1), A.strm.avail_out === 0) ? g : v;
      }
      function at(A, X) {
        for (var z, _, P; ; ) {
          if (A.lookahead < Y) {
            if (St(A), A.lookahead < Y && X === f)
              return g;
            if (A.lookahead === 0)
              break;
          }
          if (z = 0, A.lookahead >= L && (A.ins_h = (A.ins_h << A.hash_shift ^ A.window[A.strstart + L - 1]) & A.hash_mask, z = A.prev[A.strstart & A.w_mask] = A.head[A.ins_h], A.head[A.ins_h] = A.strstart), A.prev_length = A.match_length, A.prev_match = A.match_start, A.match_length = L - 1, z !== 0 && A.prev_length < A.max_lazy_match && A.strstart - z <= A.w_size - Y && (A.match_length = tt(A, z), A.match_length <= 5 && (A.strategy === 1 || A.match_length === L && 4096 < A.strstart - A.match_start) && (A.match_length = L - 1)), A.prev_length >= L && A.match_length <= A.prev_length) {
            for (P = A.strstart + A.lookahead - L, _ = o._tr_tally(A, A.strstart - 1 - A.prev_match, A.prev_length - L), A.lookahead -= A.prev_length - 1, A.prev_length -= 2; ++A.strstart <= P && (A.ins_h = (A.ins_h << A.hash_shift ^ A.window[A.strstart + L - 1]) & A.hash_mask, z = A.prev[A.strstart & A.w_mask] = A.head[A.ins_h], A.head[A.ins_h] = A.strstart), --A.prev_length != 0; )
              ;
            if (A.match_available = 0, A.match_length = L - 1, A.strstart++, _ && (x(A, !1), A.strm.avail_out === 0))
              return g;
          } else if (A.match_available) {
            if ((_ = o._tr_tally(A, 0, A.window[A.strstart - 1])) && x(A, !1), A.strstart++, A.lookahead--, A.strm.avail_out === 0)
              return g;
          } else
            A.match_available = 1, A.strstart++, A.lookahead--;
        }
        return A.match_available && (_ = o._tr_tally(A, 0, A.window[A.strstart - 1]), A.match_available = 0), A.insert = A.strstart < L - 1 ? A.strstart : L - 1, X === I ? (x(A, !0), A.strm.avail_out === 0 ? q : G) : A.last_lit && (x(A, !1), A.strm.avail_out === 0) ? g : v;
      }
      function It(A, X, z, _, P) {
        this.good_length = A, this.max_lazy = X, this.nice_length = z, this.max_chain = _, this.func = P;
      }
      function ft() {
        this.strm = null, this.status = 0, this.pending_buf = null, this.pending_buf_size = 0, this.pending_out = 0, this.pending = 0, this.wrap = 0, this.gzhead = null, this.gzindex = 0, this.method = R, this.last_flush = -1, this.w_size = 0, this.w_bits = 0, this.w_mask = 0, this.window = null, this.window_size = 0, this.prev = null, this.head = null, this.ins_h = 0, this.hash_size = 0, this.hash_bits = 0, this.hash_mask = 0, this.hash_shift = 0, this.block_start = 0, this.match_length = 0, this.prev_match = 0, this.match_available = 0, this.strstart = 0, this.match_start = 0, this.lookahead = 0, this.prev_length = 0, this.max_chain_length = 0, this.max_lazy_match = 0, this.level = 0, this.strategy = 0, this.good_match = 0, this.nice_match = 0, this.dyn_ltree = new r.Buf16(2 * y), this.dyn_dtree = new r.Buf16(2 * (2 * F + 1)), this.bl_tree = new r.Buf16(2 * (2 * O + 1)), nt(this.dyn_ltree), nt(this.dyn_dtree), nt(this.bl_tree), this.l_desc = null, this.d_desc = null, this.bl_desc = null, this.bl_count = new r.Buf16(w + 1), this.heap = new r.Buf16(2 * m + 1), nt(this.heap), this.heap_len = 0, this.heap_max = 0, this.depth = new r.Buf16(2 * m + 1), nt(this.depth), this.l_buf = 0, this.lit_bufsize = 0, this.last_lit = 0, this.d_buf = 0, this.opt_len = 0, this.static_len = 0, this.matches = 0, this.insert = 0, this.bi_buf = 0, this.bi_valid = 0;
      }
      function Ct(A) {
        var X;
        return A && A.state ? (A.total_in = A.total_out = 0, A.data_type = p, (X = A.state).pending = 0, X.pending_out = 0, X.wrap < 0 && (X.wrap = -X.wrap), X.status = X.wrap ? N : M, A.adler = X.wrap === 2 ? 0 : 1, X.last_flush = f, o._tr_init(X), u) : et(A, d);
      }
      function Ht(A) {
        var X = Ct(A);
        return X === u && function(z) {
          z.window_size = 2 * z.w_size, nt(z.head), z.max_lazy_match = n[z.level].max_lazy, z.good_match = n[z.level].good_length, z.nice_match = n[z.level].nice_length, z.max_chain_length = n[z.level].max_chain, z.strstart = 0, z.block_start = 0, z.lookahead = 0, z.insert = 0, z.match_length = z.prev_length = L - 1, z.match_available = 0, z.ins_h = 0;
        }(A.state), X;
      }
      function Dt(A, X, z, _, P, U) {
        if (!A)
          return d;
        var $ = 1;
        if (X === E && (X = 6), _ < 0 ? ($ = 0, _ = -_) : 15 < _ && ($ = 2, _ -= 16), P < 1 || S < P || z !== R || _ < 8 || 15 < _ || X < 0 || 9 < X || U < 0 || T < U)
          return et(A, d);
        _ === 8 && (_ = 9);
        var Z = new ft();
        return (A.state = Z).strm = A, Z.wrap = $, Z.gzhead = null, Z.w_bits = _, Z.w_size = 1 << Z.w_bits, Z.w_mask = Z.w_size - 1, Z.hash_bits = P + 7, Z.hash_size = 1 << Z.hash_bits, Z.hash_mask = Z.hash_size - 1, Z.hash_shift = ~~((Z.hash_bits + L - 1) / L), Z.window = new r.Buf8(2 * Z.w_size), Z.head = new r.Buf16(Z.hash_size), Z.prev = new r.Buf16(Z.w_size), Z.lit_bufsize = 1 << P + 6, Z.pending_buf_size = 4 * Z.lit_bufsize, Z.pending_buf = new r.Buf8(Z.pending_buf_size), Z.d_buf = 1 * Z.lit_bufsize, Z.l_buf = 3 * Z.lit_bufsize, Z.level = X, Z.strategy = U, Z.method = z, Ht(A);
      }
      n = [new It(0, 0, 0, 0, function(A, X) {
        var z = 65535;
        for (z > A.pending_buf_size - 5 && (z = A.pending_buf_size - 5); ; ) {
          if (A.lookahead <= 1) {
            if (St(A), A.lookahead === 0 && X === f)
              return g;
            if (A.lookahead === 0)
              break;
          }
          A.strstart += A.lookahead, A.lookahead = 0;
          var _ = A.block_start + z;
          if ((A.strstart === 0 || A.strstart >= _) && (A.lookahead = A.strstart - _, A.strstart = _, x(A, !1), A.strm.avail_out === 0) || A.strstart - A.block_start >= A.w_size - Y && (x(A, !1), A.strm.avail_out === 0))
            return g;
        }
        return A.insert = 0, X === I ? (x(A, !0), A.strm.avail_out === 0 ? q : G) : (A.strstart > A.block_start && (x(A, !1), A.strm.avail_out), g);
      }), new It(4, 4, 8, 4, yt), new It(4, 5, 16, 8, yt), new It(4, 6, 32, 32, yt), new It(4, 4, 16, 16, at), new It(8, 16, 32, 32, at), new It(8, 16, 128, 128, at), new It(8, 32, 128, 256, at), new It(32, 128, 258, 1024, at), new It(32, 258, 258, 4096, at)], s.deflateInit = function(A, X) {
        return Dt(A, X, R, 15, 8, 0);
      }, s.deflateInit2 = Dt, s.deflateReset = Ht, s.deflateResetKeep = Ct, s.deflateSetHeader = function(A, X) {
        return A && A.state ? A.state.wrap !== 2 ? d : (A.state.gzhead = X, u) : d;
      }, s.deflate = function(A, X) {
        var z, _, P, U;
        if (!A || !A.state || 5 < X || X < 0)
          return A ? et(A, d) : d;
        if (_ = A.state, !A.output || !A.input && A.avail_in !== 0 || _.status === 666 && X !== I)
          return et(A, A.avail_out === 0 ? -5 : d);
        if (_.strm = A, z = _.last_flush, _.last_flush = X, _.status === N)
          if (_.wrap === 2)
            A.adler = 0, ot(_, 31), ot(_, 139), ot(_, 8), _.gzhead ? (ot(_, (_.gzhead.text ? 1 : 0) + (_.gzhead.hcrc ? 2 : 0) + (_.gzhead.extra ? 4 : 0) + (_.gzhead.name ? 8 : 0) + (_.gzhead.comment ? 16 : 0)), ot(_, 255 & _.gzhead.time), ot(_, _.gzhead.time >> 8 & 255), ot(_, _.gzhead.time >> 16 & 255), ot(_, _.gzhead.time >> 24 & 255), ot(_, _.level === 9 ? 2 : 2 <= _.strategy || _.level < 2 ? 4 : 0), ot(_, 255 & _.gzhead.os), _.gzhead.extra && _.gzhead.extra.length && (ot(_, 255 & _.gzhead.extra.length), ot(_, _.gzhead.extra.length >> 8 & 255)), _.gzhead.hcrc && (A.adler = l(A.adler, _.pending_buf, _.pending, 0)), _.gzindex = 0, _.status = 69) : (ot(_, 0), ot(_, 0), ot(_, 0), ot(_, 0), ot(_, 0), ot(_, _.level === 9 ? 2 : 2 <= _.strategy || _.level < 2 ? 4 : 0), ot(_, 3), _.status = M);
          else {
            var $ = R + (_.w_bits - 8 << 4) << 8;
            $ |= (2 <= _.strategy || _.level < 2 ? 0 : _.level < 6 ? 1 : _.level === 6 ? 2 : 3) << 6, _.strstart !== 0 && ($ |= 32), $ += 31 - $ % 31, _.status = M, it(_, $), _.strstart !== 0 && (it(_, A.adler >>> 16), it(_, 65535 & A.adler)), A.adler = 1;
          }
        if (_.status === 69)
          if (_.gzhead.extra) {
            for (P = _.pending; _.gzindex < (65535 & _.gzhead.extra.length) && (_.pending !== _.pending_buf_size || (_.gzhead.hcrc && _.pending > P && (A.adler = l(A.adler, _.pending_buf, _.pending - P, P)), V(A), P = _.pending, _.pending !== _.pending_buf_size)); )
              ot(_, 255 & _.gzhead.extra[_.gzindex]), _.gzindex++;
            _.gzhead.hcrc && _.pending > P && (A.adler = l(A.adler, _.pending_buf, _.pending - P, P)), _.gzindex === _.gzhead.extra.length && (_.gzindex = 0, _.status = 73);
          } else
            _.status = 73;
        if (_.status === 73)
          if (_.gzhead.name) {
            P = _.pending;
            do {
              if (_.pending === _.pending_buf_size && (_.gzhead.hcrc && _.pending > P && (A.adler = l(A.adler, _.pending_buf, _.pending - P, P)), V(A), P = _.pending, _.pending === _.pending_buf_size)) {
                U = 1;
                break;
              }
              U = _.gzindex < _.gzhead.name.length ? 255 & _.gzhead.name.charCodeAt(_.gzindex++) : 0, ot(_, U);
            } while (U !== 0);
            _.gzhead.hcrc && _.pending > P && (A.adler = l(A.adler, _.pending_buf, _.pending - P, P)), U === 0 && (_.gzindex = 0, _.status = 91);
          } else
            _.status = 91;
        if (_.status === 91)
          if (_.gzhead.comment) {
            P = _.pending;
            do {
              if (_.pending === _.pending_buf_size && (_.gzhead.hcrc && _.pending > P && (A.adler = l(A.adler, _.pending_buf, _.pending - P, P)), V(A), P = _.pending, _.pending === _.pending_buf_size)) {
                U = 1;
                break;
              }
              U = _.gzindex < _.gzhead.comment.length ? 255 & _.gzhead.comment.charCodeAt(_.gzindex++) : 0, ot(_, U);
            } while (U !== 0);
            _.gzhead.hcrc && _.pending > P && (A.adler = l(A.adler, _.pending_buf, _.pending - P, P)), U === 0 && (_.status = 103);
          } else
            _.status = 103;
        if (_.status === 103 && (_.gzhead.hcrc ? (_.pending + 2 > _.pending_buf_size && V(A), _.pending + 2 <= _.pending_buf_size && (ot(_, 255 & A.adler), ot(_, A.adler >> 8 & 255), A.adler = 0, _.status = M)) : _.status = M), _.pending !== 0) {
          if (V(A), A.avail_out === 0)
            return _.last_flush = -1, u;
        } else if (A.avail_in === 0 && W(X) <= W(z) && X !== I)
          return et(A, -5);
        if (_.status === 666 && A.avail_in !== 0)
          return et(A, -5);
        if (A.avail_in !== 0 || _.lookahead !== 0 || X !== f && _.status !== 666) {
          var Z = _.strategy === 2 ? function(B, K) {
            for (var st; ; ) {
              if (B.lookahead === 0 && (St(B), B.lookahead === 0)) {
                if (K === f)
                  return g;
                break;
              }
              if (B.match_length = 0, st = o._tr_tally(B, 0, B.window[B.strstart]), B.lookahead--, B.strstart++, st && (x(B, !1), B.strm.avail_out === 0))
                return g;
            }
            return B.insert = 0, K === I ? (x(B, !0), B.strm.avail_out === 0 ? q : G) : B.last_lit && (x(B, !1), B.strm.avail_out === 0) ? g : v;
          }(_, X) : _.strategy === 3 ? function(B, K) {
            for (var st, J, ct, Rt, dt = B.window; ; ) {
              if (B.lookahead <= b) {
                if (St(B), B.lookahead <= b && K === f)
                  return g;
                if (B.lookahead === 0)
                  break;
              }
              if (B.match_length = 0, B.lookahead >= L && 0 < B.strstart && (J = dt[ct = B.strstart - 1]) === dt[++ct] && J === dt[++ct] && J === dt[++ct]) {
                Rt = B.strstart + b;
                do
                  ;
                while (J === dt[++ct] && J === dt[++ct] && J === dt[++ct] && J === dt[++ct] && J === dt[++ct] && J === dt[++ct] && J === dt[++ct] && J === dt[++ct] && ct < Rt);
                B.match_length = b - (Rt - ct), B.match_length > B.lookahead && (B.match_length = B.lookahead);
              }
              if (B.match_length >= L ? (st = o._tr_tally(B, 1, B.match_length - L), B.lookahead -= B.match_length, B.strstart += B.match_length, B.match_length = 0) : (st = o._tr_tally(B, 0, B.window[B.strstart]), B.lookahead--, B.strstart++), st && (x(B, !1), B.strm.avail_out === 0))
                return g;
            }
            return B.insert = 0, K === I ? (x(B, !0), B.strm.avail_out === 0 ? q : G) : B.last_lit && (x(B, !1), B.strm.avail_out === 0) ? g : v;
          }(_, X) : n[_.level].func(_, X);
          if (Z !== q && Z !== G || (_.status = 666), Z === g || Z === q)
            return A.avail_out === 0 && (_.last_flush = -1), u;
          if (Z === v && (X === 1 ? o._tr_align(_) : X !== 5 && (o._tr_stored_block(_, 0, 0, !1), X === 3 && (nt(_.head), _.lookahead === 0 && (_.strstart = 0, _.block_start = 0, _.insert = 0))), V(A), A.avail_out === 0))
            return _.last_flush = -1, u;
        }
        return X !== I ? u : _.wrap <= 0 ? 1 : (_.wrap === 2 ? (ot(_, 255 & A.adler), ot(_, A.adler >> 8 & 255), ot(_, A.adler >> 16 & 255), ot(_, A.adler >> 24 & 255), ot(_, 255 & A.total_in), ot(_, A.total_in >> 8 & 255), ot(_, A.total_in >> 16 & 255), ot(_, A.total_in >> 24 & 255)) : (it(_, A.adler >>> 16), it(_, 65535 & A.adler)), V(A), 0 < _.wrap && (_.wrap = -_.wrap), _.pending !== 0 ? u : 1);
      }, s.deflateEnd = function(A) {
        var X;
        return A && A.state ? (X = A.state.status) !== N && X !== 69 && X !== 73 && X !== 91 && X !== 103 && X !== M && X !== 666 ? et(A, d) : (A.state = null, X === M ? et(A, -3) : u) : d;
      }, s.deflateSetDictionary = function(A, X) {
        var z, _, P, U, $, Z, B, K, st = X.length;
        if (!A || !A.state || (U = (z = A.state).wrap) === 2 || U === 1 && z.status !== N || z.lookahead)
          return d;
        for (U === 1 && (A.adler = a(A.adler, X, st, 0)), z.wrap = 0, st >= z.w_size && (U === 0 && (nt(z.head), z.strstart = 0, z.block_start = 0, z.insert = 0), K = new r.Buf8(z.w_size), r.arraySet(K, X, st - z.w_size, z.w_size, 0), X = K, st = z.w_size), $ = A.avail_in, Z = A.next_in, B = A.input, A.avail_in = st, A.next_in = 0, A.input = X, St(z); z.lookahead >= L; ) {
          for (_ = z.strstart, P = z.lookahead - (L - 1); z.ins_h = (z.ins_h << z.hash_shift ^ z.window[_ + L - 1]) & z.hash_mask, z.prev[_ & z.w_mask] = z.head[z.ins_h], z.head[z.ins_h] = _, _++, --P; )
            ;
          z.strstart = _, z.lookahead = L - 1, St(z);
        }
        return z.strstart += z.lookahead, z.block_start = z.strstart, z.insert = z.lookahead, z.lookahead = 0, z.match_length = z.prev_length = L - 1, z.match_available = 0, A.next_in = Z, A.input = B, A.avail_in = $, z.wrap = U, u;
      }, s.deflateInfo = "pako deflate (from Nodeca project)";
    }, { "../utils/common": 41, "./adler32": 43, "./crc32": 45, "./messages": 51, "./trees": 52 }], 47: [function(t, e, s) {
      e.exports = function() {
        this.text = 0, this.time = 0, this.xflags = 0, this.os = 0, this.extra = null, this.extra_len = 0, this.name = "", this.comment = "", this.hcrc = 0, this.done = !1;
      };
    }, {}], 48: [function(t, e, s) {
      e.exports = function(n, r) {
        var o, a, l, h, f, I, u, d, E, T, p, R, S, m, F, O, y, w, L, b, Y, N, M, g, v;
        o = n.state, a = n.next_in, g = n.input, l = a + (n.avail_in - 5), h = n.next_out, v = n.output, f = h - (r - n.avail_out), I = h + (n.avail_out - 257), u = o.dmax, d = o.wsize, E = o.whave, T = o.wnext, p = o.window, R = o.hold, S = o.bits, m = o.lencode, F = o.distcode, O = (1 << o.lenbits) - 1, y = (1 << o.distbits) - 1;
        t:
          do {
            S < 15 && (R += g[a++] << S, S += 8, R += g[a++] << S, S += 8), w = m[R & O];
            e:
              for (; ; ) {
                if (R >>>= L = w >>> 24, S -= L, (L = w >>> 16 & 255) === 0)
                  v[h++] = 65535 & w;
                else {
                  if (!(16 & L)) {
                    if (!(64 & L)) {
                      w = m[(65535 & w) + (R & (1 << L) - 1)];
                      continue e;
                    }
                    if (32 & L) {
                      o.mode = 12;
                      break t;
                    }
                    n.msg = "invalid literal/length code", o.mode = 30;
                    break t;
                  }
                  b = 65535 & w, (L &= 15) && (S < L && (R += g[a++] << S, S += 8), b += R & (1 << L) - 1, R >>>= L, S -= L), S < 15 && (R += g[a++] << S, S += 8, R += g[a++] << S, S += 8), w = F[R & y];
                  i:
                    for (; ; ) {
                      if (R >>>= L = w >>> 24, S -= L, !(16 & (L = w >>> 16 & 255))) {
                        if (!(64 & L)) {
                          w = F[(65535 & w) + (R & (1 << L) - 1)];
                          continue i;
                        }
                        n.msg = "invalid distance code", o.mode = 30;
                        break t;
                      }
                      if (Y = 65535 & w, S < (L &= 15) && (R += g[a++] << S, (S += 8) < L && (R += g[a++] << S, S += 8)), u < (Y += R & (1 << L) - 1)) {
                        n.msg = "invalid distance too far back", o.mode = 30;
                        break t;
                      }
                      if (R >>>= L, S -= L, (L = h - f) < Y) {
                        if (E < (L = Y - L) && o.sane) {
                          n.msg = "invalid distance too far back", o.mode = 30;
                          break t;
                        }
                        if (M = p, (N = 0) === T) {
                          if (N += d - L, L < b) {
                            for (b -= L; v[h++] = p[N++], --L; )
                              ;
                            N = h - Y, M = v;
                          }
                        } else if (T < L) {
                          if (N += d + T - L, (L -= T) < b) {
                            for (b -= L; v[h++] = p[N++], --L; )
                              ;
                            if (N = 0, T < b) {
                              for (b -= L = T; v[h++] = p[N++], --L; )
                                ;
                              N = h - Y, M = v;
                            }
                          }
                        } else if (N += T - L, L < b) {
                          for (b -= L; v[h++] = p[N++], --L; )
                            ;
                          N = h - Y, M = v;
                        }
                        for (; 2 < b; )
                          v[h++] = M[N++], v[h++] = M[N++], v[h++] = M[N++], b -= 3;
                        b && (v[h++] = M[N++], 1 < b && (v[h++] = M[N++]));
                      } else {
                        for (N = h - Y; v[h++] = v[N++], v[h++] = v[N++], v[h++] = v[N++], 2 < (b -= 3); )
                          ;
                        b && (v[h++] = v[N++], 1 < b && (v[h++] = v[N++]));
                      }
                      break;
                    }
                }
                break;
              }
          } while (a < l && h < I);
        a -= b = S >> 3, R &= (1 << (S -= b << 3)) - 1, n.next_in = a, n.next_out = h, n.avail_in = a < l ? l - a + 5 : 5 - (a - l), n.avail_out = h < I ? I - h + 257 : 257 - (h - I), o.hold = R, o.bits = S;
      };
    }, {}], 49: [function(t, e, s) {
      var n = t("../utils/common"), r = t("./adler32"), o = t("./crc32"), a = t("./inffast"), l = t("./inftrees"), h = 1, f = 2, I = 0, u = -2, d = 1, E = 852, T = 592;
      function p(N) {
        return (N >>> 24 & 255) + (N >>> 8 & 65280) + ((65280 & N) << 8) + ((255 & N) << 24);
      }
      function R() {
        this.mode = 0, this.last = !1, this.wrap = 0, this.havedict = !1, this.flags = 0, this.dmax = 0, this.check = 0, this.total = 0, this.head = null, this.wbits = 0, this.wsize = 0, this.whave = 0, this.wnext = 0, this.window = null, this.hold = 0, this.bits = 0, this.length = 0, this.offset = 0, this.extra = 0, this.lencode = null, this.distcode = null, this.lenbits = 0, this.distbits = 0, this.ncode = 0, this.nlen = 0, this.ndist = 0, this.have = 0, this.next = null, this.lens = new n.Buf16(320), this.work = new n.Buf16(288), this.lendyn = null, this.distdyn = null, this.sane = 0, this.back = 0, this.was = 0;
      }
      function S(N) {
        var M;
        return N && N.state ? (M = N.state, N.total_in = N.total_out = M.total = 0, N.msg = "", M.wrap && (N.adler = 1 & M.wrap), M.mode = d, M.last = 0, M.havedict = 0, M.dmax = 32768, M.head = null, M.hold = 0, M.bits = 0, M.lencode = M.lendyn = new n.Buf32(E), M.distcode = M.distdyn = new n.Buf32(T), M.sane = 1, M.back = -1, I) : u;
      }
      function m(N) {
        var M;
        return N && N.state ? ((M = N.state).wsize = 0, M.whave = 0, M.wnext = 0, S(N)) : u;
      }
      function F(N, M) {
        var g, v;
        return N && N.state ? (v = N.state, M < 0 ? (g = 0, M = -M) : (g = 1 + (M >> 4), M < 48 && (M &= 15)), M && (M < 8 || 15 < M) ? u : (v.window !== null && v.wbits !== M && (v.window = null), v.wrap = g, v.wbits = M, m(N))) : u;
      }
      function O(N, M) {
        var g, v;
        return N ? (v = new R(), (N.state = v).window = null, (g = F(N, M)) !== I && (N.state = null), g) : u;
      }
      var y, w, L = !0;
      function b(N) {
        if (L) {
          var M;
          for (y = new n.Buf32(512), w = new n.Buf32(32), M = 0; M < 144; )
            N.lens[M++] = 8;
          for (; M < 256; )
            N.lens[M++] = 9;
          for (; M < 280; )
            N.lens[M++] = 7;
          for (; M < 288; )
            N.lens[M++] = 8;
          for (l(h, N.lens, 0, 288, y, 0, N.work, { bits: 9 }), M = 0; M < 32; )
            N.lens[M++] = 5;
          l(f, N.lens, 0, 32, w, 0, N.work, { bits: 5 }), L = !1;
        }
        N.lencode = y, N.lenbits = 9, N.distcode = w, N.distbits = 5;
      }
      function Y(N, M, g, v) {
        var q, G = N.state;
        return G.window === null && (G.wsize = 1 << G.wbits, G.wnext = 0, G.whave = 0, G.window = new n.Buf8(G.wsize)), v >= G.wsize ? (n.arraySet(G.window, M, g - G.wsize, G.wsize, 0), G.wnext = 0, G.whave = G.wsize) : (v < (q = G.wsize - G.wnext) && (q = v), n.arraySet(G.window, M, g - v, q, G.wnext), (v -= q) ? (n.arraySet(G.window, M, g - v, v, 0), G.wnext = v, G.whave = G.wsize) : (G.wnext += q, G.wnext === G.wsize && (G.wnext = 0), G.whave < G.wsize && (G.whave += q))), 0;
      }
      s.inflateReset = m, s.inflateReset2 = F, s.inflateResetKeep = S, s.inflateInit = function(N) {
        return O(N, 15);
      }, s.inflateInit2 = O, s.inflate = function(N, M) {
        var g, v, q, G, et, W, nt, V, x, ot, it, tt, St, yt, at, It, ft, Ct, Ht, Dt, A, X, z, _, P = 0, U = new n.Buf8(4), $ = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15];
        if (!N || !N.state || !N.output || !N.input && N.avail_in !== 0)
          return u;
        (g = N.state).mode === 12 && (g.mode = 13), et = N.next_out, q = N.output, nt = N.avail_out, G = N.next_in, v = N.input, W = N.avail_in, V = g.hold, x = g.bits, ot = W, it = nt, X = I;
        t:
          for (; ; )
            switch (g.mode) {
              case d:
                if (g.wrap === 0) {
                  g.mode = 13;
                  break;
                }
                for (; x < 16; ) {
                  if (W === 0)
                    break t;
                  W--, V += v[G++] << x, x += 8;
                }
                if (2 & g.wrap && V === 35615) {
                  U[g.check = 0] = 255 & V, U[1] = V >>> 8 & 255, g.check = o(g.check, U, 2, 0), x = V = 0, g.mode = 2;
                  break;
                }
                if (g.flags = 0, g.head && (g.head.done = !1), !(1 & g.wrap) || (((255 & V) << 8) + (V >> 8)) % 31) {
                  N.msg = "incorrect header check", g.mode = 30;
                  break;
                }
                if ((15 & V) != 8) {
                  N.msg = "unknown compression method", g.mode = 30;
                  break;
                }
                if (x -= 4, A = 8 + (15 & (V >>>= 4)), g.wbits === 0)
                  g.wbits = A;
                else if (A > g.wbits) {
                  N.msg = "invalid window size", g.mode = 30;
                  break;
                }
                g.dmax = 1 << A, N.adler = g.check = 1, g.mode = 512 & V ? 10 : 12, x = V = 0;
                break;
              case 2:
                for (; x < 16; ) {
                  if (W === 0)
                    break t;
                  W--, V += v[G++] << x, x += 8;
                }
                if (g.flags = V, (255 & g.flags) != 8) {
                  N.msg = "unknown compression method", g.mode = 30;
                  break;
                }
                if (57344 & g.flags) {
                  N.msg = "unknown header flags set", g.mode = 30;
                  break;
                }
                g.head && (g.head.text = V >> 8 & 1), 512 & g.flags && (U[0] = 255 & V, U[1] = V >>> 8 & 255, g.check = o(g.check, U, 2, 0)), x = V = 0, g.mode = 3;
              case 3:
                for (; x < 32; ) {
                  if (W === 0)
                    break t;
                  W--, V += v[G++] << x, x += 8;
                }
                g.head && (g.head.time = V), 512 & g.flags && (U[0] = 255 & V, U[1] = V >>> 8 & 255, U[2] = V >>> 16 & 255, U[3] = V >>> 24 & 255, g.check = o(g.check, U, 4, 0)), x = V = 0, g.mode = 4;
              case 4:
                for (; x < 16; ) {
                  if (W === 0)
                    break t;
                  W--, V += v[G++] << x, x += 8;
                }
                g.head && (g.head.xflags = 255 & V, g.head.os = V >> 8), 512 & g.flags && (U[0] = 255 & V, U[1] = V >>> 8 & 255, g.check = o(g.check, U, 2, 0)), x = V = 0, g.mode = 5;
              case 5:
                if (1024 & g.flags) {
                  for (; x < 16; ) {
                    if (W === 0)
                      break t;
                    W--, V += v[G++] << x, x += 8;
                  }
                  g.length = V, g.head && (g.head.extra_len = V), 512 & g.flags && (U[0] = 255 & V, U[1] = V >>> 8 & 255, g.check = o(g.check, U, 2, 0)), x = V = 0;
                } else
                  g.head && (g.head.extra = null);
                g.mode = 6;
              case 6:
                if (1024 & g.flags && (W < (tt = g.length) && (tt = W), tt && (g.head && (A = g.head.extra_len - g.length, g.head.extra || (g.head.extra = new Array(g.head.extra_len)), n.arraySet(g.head.extra, v, G, tt, A)), 512 & g.flags && (g.check = o(g.check, v, tt, G)), W -= tt, G += tt, g.length -= tt), g.length))
                  break t;
                g.length = 0, g.mode = 7;
              case 7:
                if (2048 & g.flags) {
                  if (W === 0)
                    break t;
                  for (tt = 0; A = v[G + tt++], g.head && A && g.length < 65536 && (g.head.name += String.fromCharCode(A)), A && tt < W; )
                    ;
                  if (512 & g.flags && (g.check = o(g.check, v, tt, G)), W -= tt, G += tt, A)
                    break t;
                } else
                  g.head && (g.head.name = null);
                g.length = 0, g.mode = 8;
              case 8:
                if (4096 & g.flags) {
                  if (W === 0)
                    break t;
                  for (tt = 0; A = v[G + tt++], g.head && A && g.length < 65536 && (g.head.comment += String.fromCharCode(A)), A && tt < W; )
                    ;
                  if (512 & g.flags && (g.check = o(g.check, v, tt, G)), W -= tt, G += tt, A)
                    break t;
                } else
                  g.head && (g.head.comment = null);
                g.mode = 9;
              case 9:
                if (512 & g.flags) {
                  for (; x < 16; ) {
                    if (W === 0)
                      break t;
                    W--, V += v[G++] << x, x += 8;
                  }
                  if (V !== (65535 & g.check)) {
                    N.msg = "header crc mismatch", g.mode = 30;
                    break;
                  }
                  x = V = 0;
                }
                g.head && (g.head.hcrc = g.flags >> 9 & 1, g.head.done = !0), N.adler = g.check = 0, g.mode = 12;
                break;
              case 10:
                for (; x < 32; ) {
                  if (W === 0)
                    break t;
                  W--, V += v[G++] << x, x += 8;
                }
                N.adler = g.check = p(V), x = V = 0, g.mode = 11;
              case 11:
                if (g.havedict === 0)
                  return N.next_out = et, N.avail_out = nt, N.next_in = G, N.avail_in = W, g.hold = V, g.bits = x, 2;
                N.adler = g.check = 1, g.mode = 12;
              case 12:
                if (M === 5 || M === 6)
                  break t;
              case 13:
                if (g.last) {
                  V >>>= 7 & x, x -= 7 & x, g.mode = 27;
                  break;
                }
                for (; x < 3; ) {
                  if (W === 0)
                    break t;
                  W--, V += v[G++] << x, x += 8;
                }
                switch (g.last = 1 & V, x -= 1, 3 & (V >>>= 1)) {
                  case 0:
                    g.mode = 14;
                    break;
                  case 1:
                    if (b(g), g.mode = 20, M !== 6)
                      break;
                    V >>>= 2, x -= 2;
                    break t;
                  case 2:
                    g.mode = 17;
                    break;
                  case 3:
                    N.msg = "invalid block type", g.mode = 30;
                }
                V >>>= 2, x -= 2;
                break;
              case 14:
                for (V >>>= 7 & x, x -= 7 & x; x < 32; ) {
                  if (W === 0)
                    break t;
                  W--, V += v[G++] << x, x += 8;
                }
                if ((65535 & V) != (V >>> 16 ^ 65535)) {
                  N.msg = "invalid stored block lengths", g.mode = 30;
                  break;
                }
                if (g.length = 65535 & V, x = V = 0, g.mode = 15, M === 6)
                  break t;
              case 15:
                g.mode = 16;
              case 16:
                if (tt = g.length) {
                  if (W < tt && (tt = W), nt < tt && (tt = nt), tt === 0)
                    break t;
                  n.arraySet(q, v, G, tt, et), W -= tt, G += tt, nt -= tt, et += tt, g.length -= tt;
                  break;
                }
                g.mode = 12;
                break;
              case 17:
                for (; x < 14; ) {
                  if (W === 0)
                    break t;
                  W--, V += v[G++] << x, x += 8;
                }
                if (g.nlen = 257 + (31 & V), V >>>= 5, x -= 5, g.ndist = 1 + (31 & V), V >>>= 5, x -= 5, g.ncode = 4 + (15 & V), V >>>= 4, x -= 4, 286 < g.nlen || 30 < g.ndist) {
                  N.msg = "too many length or distance symbols", g.mode = 30;
                  break;
                }
                g.have = 0, g.mode = 18;
              case 18:
                for (; g.have < g.ncode; ) {
                  for (; x < 3; ) {
                    if (W === 0)
                      break t;
                    W--, V += v[G++] << x, x += 8;
                  }
                  g.lens[$[g.have++]] = 7 & V, V >>>= 3, x -= 3;
                }
                for (; g.have < 19; )
                  g.lens[$[g.have++]] = 0;
                if (g.lencode = g.lendyn, g.lenbits = 7, z = { bits: g.lenbits }, X = l(0, g.lens, 0, 19, g.lencode, 0, g.work, z), g.lenbits = z.bits, X) {
                  N.msg = "invalid code lengths set", g.mode = 30;
                  break;
                }
                g.have = 0, g.mode = 19;
              case 19:
                for (; g.have < g.nlen + g.ndist; ) {
                  for (; It = (P = g.lencode[V & (1 << g.lenbits) - 1]) >>> 16 & 255, ft = 65535 & P, !((at = P >>> 24) <= x); ) {
                    if (W === 0)
                      break t;
                    W--, V += v[G++] << x, x += 8;
                  }
                  if (ft < 16)
                    V >>>= at, x -= at, g.lens[g.have++] = ft;
                  else {
                    if (ft === 16) {
                      for (_ = at + 2; x < _; ) {
                        if (W === 0)
                          break t;
                        W--, V += v[G++] << x, x += 8;
                      }
                      if (V >>>= at, x -= at, g.have === 0) {
                        N.msg = "invalid bit length repeat", g.mode = 30;
                        break;
                      }
                      A = g.lens[g.have - 1], tt = 3 + (3 & V), V >>>= 2, x -= 2;
                    } else if (ft === 17) {
                      for (_ = at + 3; x < _; ) {
                        if (W === 0)
                          break t;
                        W--, V += v[G++] << x, x += 8;
                      }
                      x -= at, A = 0, tt = 3 + (7 & (V >>>= at)), V >>>= 3, x -= 3;
                    } else {
                      for (_ = at + 7; x < _; ) {
                        if (W === 0)
                          break t;
                        W--, V += v[G++] << x, x += 8;
                      }
                      x -= at, A = 0, tt = 11 + (127 & (V >>>= at)), V >>>= 7, x -= 7;
                    }
                    if (g.have + tt > g.nlen + g.ndist) {
                      N.msg = "invalid bit length repeat", g.mode = 30;
                      break;
                    }
                    for (; tt--; )
                      g.lens[g.have++] = A;
                  }
                }
                if (g.mode === 30)
                  break;
                if (g.lens[256] === 0) {
                  N.msg = "invalid code -- missing end-of-block", g.mode = 30;
                  break;
                }
                if (g.lenbits = 9, z = { bits: g.lenbits }, X = l(h, g.lens, 0, g.nlen, g.lencode, 0, g.work, z), g.lenbits = z.bits, X) {
                  N.msg = "invalid literal/lengths set", g.mode = 30;
                  break;
                }
                if (g.distbits = 6, g.distcode = g.distdyn, z = { bits: g.distbits }, X = l(f, g.lens, g.nlen, g.ndist, g.distcode, 0, g.work, z), g.distbits = z.bits, X) {
                  N.msg = "invalid distances set", g.mode = 30;
                  break;
                }
                if (g.mode = 20, M === 6)
                  break t;
              case 20:
                g.mode = 21;
              case 21:
                if (6 <= W && 258 <= nt) {
                  N.next_out = et, N.avail_out = nt, N.next_in = G, N.avail_in = W, g.hold = V, g.bits = x, a(N, it), et = N.next_out, q = N.output, nt = N.avail_out, G = N.next_in, v = N.input, W = N.avail_in, V = g.hold, x = g.bits, g.mode === 12 && (g.back = -1);
                  break;
                }
                for (g.back = 0; It = (P = g.lencode[V & (1 << g.lenbits) - 1]) >>> 16 & 255, ft = 65535 & P, !((at = P >>> 24) <= x); ) {
                  if (W === 0)
                    break t;
                  W--, V += v[G++] << x, x += 8;
                }
                if (It && !(240 & It)) {
                  for (Ct = at, Ht = It, Dt = ft; It = (P = g.lencode[Dt + ((V & (1 << Ct + Ht) - 1) >> Ct)]) >>> 16 & 255, ft = 65535 & P, !(Ct + (at = P >>> 24) <= x); ) {
                    if (W === 0)
                      break t;
                    W--, V += v[G++] << x, x += 8;
                  }
                  V >>>= Ct, x -= Ct, g.back += Ct;
                }
                if (V >>>= at, x -= at, g.back += at, g.length = ft, It === 0) {
                  g.mode = 26;
                  break;
                }
                if (32 & It) {
                  g.back = -1, g.mode = 12;
                  break;
                }
                if (64 & It) {
                  N.msg = "invalid literal/length code", g.mode = 30;
                  break;
                }
                g.extra = 15 & It, g.mode = 22;
              case 22:
                if (g.extra) {
                  for (_ = g.extra; x < _; ) {
                    if (W === 0)
                      break t;
                    W--, V += v[G++] << x, x += 8;
                  }
                  g.length += V & (1 << g.extra) - 1, V >>>= g.extra, x -= g.extra, g.back += g.extra;
                }
                g.was = g.length, g.mode = 23;
              case 23:
                for (; It = (P = g.distcode[V & (1 << g.distbits) - 1]) >>> 16 & 255, ft = 65535 & P, !((at = P >>> 24) <= x); ) {
                  if (W === 0)
                    break t;
                  W--, V += v[G++] << x, x += 8;
                }
                if (!(240 & It)) {
                  for (Ct = at, Ht = It, Dt = ft; It = (P = g.distcode[Dt + ((V & (1 << Ct + Ht) - 1) >> Ct)]) >>> 16 & 255, ft = 65535 & P, !(Ct + (at = P >>> 24) <= x); ) {
                    if (W === 0)
                      break t;
                    W--, V += v[G++] << x, x += 8;
                  }
                  V >>>= Ct, x -= Ct, g.back += Ct;
                }
                if (V >>>= at, x -= at, g.back += at, 64 & It) {
                  N.msg = "invalid distance code", g.mode = 30;
                  break;
                }
                g.offset = ft, g.extra = 15 & It, g.mode = 24;
              case 24:
                if (g.extra) {
                  for (_ = g.extra; x < _; ) {
                    if (W === 0)
                      break t;
                    W--, V += v[G++] << x, x += 8;
                  }
                  g.offset += V & (1 << g.extra) - 1, V >>>= g.extra, x -= g.extra, g.back += g.extra;
                }
                if (g.offset > g.dmax) {
                  N.msg = "invalid distance too far back", g.mode = 30;
                  break;
                }
                g.mode = 25;
              case 25:
                if (nt === 0)
                  break t;
                if (tt = it - nt, g.offset > tt) {
                  if ((tt = g.offset - tt) > g.whave && g.sane) {
                    N.msg = "invalid distance too far back", g.mode = 30;
                    break;
                  }
                  St = tt > g.wnext ? (tt -= g.wnext, g.wsize - tt) : g.wnext - tt, tt > g.length && (tt = g.length), yt = g.window;
                } else
                  yt = q, St = et - g.offset, tt = g.length;
                for (nt < tt && (tt = nt), nt -= tt, g.length -= tt; q[et++] = yt[St++], --tt; )
                  ;
                g.length === 0 && (g.mode = 21);
                break;
              case 26:
                if (nt === 0)
                  break t;
                q[et++] = g.length, nt--, g.mode = 21;
                break;
              case 27:
                if (g.wrap) {
                  for (; x < 32; ) {
                    if (W === 0)
                      break t;
                    W--, V |= v[G++] << x, x += 8;
                  }
                  if (it -= nt, N.total_out += it, g.total += it, it && (N.adler = g.check = g.flags ? o(g.check, q, it, et - it) : r(g.check, q, it, et - it)), it = nt, (g.flags ? V : p(V)) !== g.check) {
                    N.msg = "incorrect data check", g.mode = 30;
                    break;
                  }
                  x = V = 0;
                }
                g.mode = 28;
              case 28:
                if (g.wrap && g.flags) {
                  for (; x < 32; ) {
                    if (W === 0)
                      break t;
                    W--, V += v[G++] << x, x += 8;
                  }
                  if (V !== (4294967295 & g.total)) {
                    N.msg = "incorrect length check", g.mode = 30;
                    break;
                  }
                  x = V = 0;
                }
                g.mode = 29;
              case 29:
                X = 1;
                break t;
              case 30:
                X = -3;
                break t;
              case 31:
                return -4;
              case 32:
              default:
                return u;
            }
        return N.next_out = et, N.avail_out = nt, N.next_in = G, N.avail_in = W, g.hold = V, g.bits = x, (g.wsize || it !== N.avail_out && g.mode < 30 && (g.mode < 27 || M !== 4)) && Y(N, N.output, N.next_out, it - N.avail_out) ? (g.mode = 31, -4) : (ot -= N.avail_in, it -= N.avail_out, N.total_in += ot, N.total_out += it, g.total += it, g.wrap && it && (N.adler = g.check = g.flags ? o(g.check, q, it, N.next_out - it) : r(g.check, q, it, N.next_out - it)), N.data_type = g.bits + (g.last ? 64 : 0) + (g.mode === 12 ? 128 : 0) + (g.mode === 20 || g.mode === 15 ? 256 : 0), (ot == 0 && it === 0 || M === 4) && X === I && (X = -5), X);
      }, s.inflateEnd = function(N) {
        if (!N || !N.state)
          return u;
        var M = N.state;
        return M.window && (M.window = null), N.state = null, I;
      }, s.inflateGetHeader = function(N, M) {
        var g;
        return N && N.state && 2 & (g = N.state).wrap ? ((g.head = M).done = !1, I) : u;
      }, s.inflateSetDictionary = function(N, M) {
        var g, v = M.length;
        return N && N.state ? (g = N.state).wrap !== 0 && g.mode !== 11 ? u : g.mode === 11 && r(1, M, v, 0) !== g.check ? -3 : Y(N, M, v, v) ? (g.mode = 31, -4) : (g.havedict = 1, I) : u;
      }, s.inflateInfo = "pako inflate (from Nodeca project)";
    }, { "../utils/common": 41, "./adler32": 43, "./crc32": 45, "./inffast": 48, "./inftrees": 50 }], 50: [function(t, e, s) {
      var n = t("../utils/common"), r = [3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258, 0, 0], o = [16, 16, 16, 16, 16, 16, 16, 16, 17, 17, 17, 17, 18, 18, 18, 18, 19, 19, 19, 19, 20, 20, 20, 20, 21, 21, 21, 21, 16, 72, 78], a = [1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577, 0, 0], l = [16, 16, 16, 16, 17, 17, 18, 18, 19, 19, 20, 20, 21, 21, 22, 22, 23, 23, 24, 24, 25, 25, 26, 26, 27, 27, 28, 28, 29, 29, 64, 64];
      e.exports = function(h, f, I, u, d, E, T, p) {
        var R, S, m, F, O, y, w, L, b, Y = p.bits, N = 0, M = 0, g = 0, v = 0, q = 0, G = 0, et = 0, W = 0, nt = 0, V = 0, x = null, ot = 0, it = new n.Buf16(16), tt = new n.Buf16(16), St = null, yt = 0;
        for (N = 0; N <= 15; N++)
          it[N] = 0;
        for (M = 0; M < u; M++)
          it[f[I + M]]++;
        for (q = Y, v = 15; 1 <= v && it[v] === 0; v--)
          ;
        if (v < q && (q = v), v === 0)
          return d[E++] = 20971520, d[E++] = 20971520, p.bits = 1, 0;
        for (g = 1; g < v && it[g] === 0; g++)
          ;
        for (q < g && (q = g), N = W = 1; N <= 15; N++)
          if (W <<= 1, (W -= it[N]) < 0)
            return -1;
        if (0 < W && (h === 0 || v !== 1))
          return -1;
        for (tt[1] = 0, N = 1; N < 15; N++)
          tt[N + 1] = tt[N] + it[N];
        for (M = 0; M < u; M++)
          f[I + M] !== 0 && (T[tt[f[I + M]]++] = M);
        if (y = h === 0 ? (x = St = T, 19) : h === 1 ? (x = r, ot -= 257, St = o, yt -= 257, 256) : (x = a, St = l, -1), N = g, O = E, et = M = V = 0, m = -1, F = (nt = 1 << (G = q)) - 1, h === 1 && 852 < nt || h === 2 && 592 < nt)
          return 1;
        for (; ; ) {
          for (w = N - et, b = T[M] < y ? (L = 0, T[M]) : T[M] > y ? (L = St[yt + T[M]], x[ot + T[M]]) : (L = 96, 0), R = 1 << N - et, g = S = 1 << G; d[O + (V >> et) + (S -= R)] = w << 24 | L << 16 | b | 0, S !== 0; )
            ;
          for (R = 1 << N - 1; V & R; )
            R >>= 1;
          if (R !== 0 ? (V &= R - 1, V += R) : V = 0, M++, --it[N] == 0) {
            if (N === v)
              break;
            N = f[I + T[M]];
          }
          if (q < N && (V & F) !== m) {
            for (et === 0 && (et = q), O += g, W = 1 << (G = N - et); G + et < v && !((W -= it[G + et]) <= 0); )
              G++, W <<= 1;
            if (nt += 1 << G, h === 1 && 852 < nt || h === 2 && 592 < nt)
              return 1;
            d[m = V & F] = q << 24 | G << 16 | O - E | 0;
          }
        }
        return V !== 0 && (d[O + V] = N - et << 24 | 64 << 16 | 0), p.bits = q, 0;
      };
    }, { "../utils/common": 41 }], 51: [function(t, e, s) {
      e.exports = { 2: "need dictionary", 1: "stream end", 0: "", "-1": "file error", "-2": "stream error", "-3": "data error", "-4": "insufficient memory", "-5": "buffer error", "-6": "incompatible version" };
    }, {}], 52: [function(t, e, s) {
      var n = t("../utils/common"), r = 0, o = 1;
      function a(P) {
        for (var U = P.length; 0 <= --U; )
          P[U] = 0;
      }
      var l = 0, h = 29, f = 256, I = f + 1 + h, u = 30, d = 19, E = 2 * I + 1, T = 15, p = 16, R = 7, S = 256, m = 16, F = 17, O = 18, y = [0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0], w = [0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13], L = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 3, 7], b = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15], Y = new Array(2 * (I + 2));
      a(Y);
      var N = new Array(2 * u);
      a(N);
      var M = new Array(512);
      a(M);
      var g = new Array(256);
      a(g);
      var v = new Array(h);
      a(v);
      var q, G, et, W = new Array(u);
      function nt(P, U, $, Z, B) {
        this.static_tree = P, this.extra_bits = U, this.extra_base = $, this.elems = Z, this.max_length = B, this.has_stree = P && P.length;
      }
      function V(P, U) {
        this.dyn_tree = P, this.max_code = 0, this.stat_desc = U;
      }
      function x(P) {
        return P < 256 ? M[P] : M[256 + (P >>> 7)];
      }
      function ot(P, U) {
        P.pending_buf[P.pending++] = 255 & U, P.pending_buf[P.pending++] = U >>> 8 & 255;
      }
      function it(P, U, $) {
        P.bi_valid > p - $ ? (P.bi_buf |= U << P.bi_valid & 65535, ot(P, P.bi_buf), P.bi_buf = U >> p - P.bi_valid, P.bi_valid += $ - p) : (P.bi_buf |= U << P.bi_valid & 65535, P.bi_valid += $);
      }
      function tt(P, U, $) {
        it(P, $[2 * U], $[2 * U + 1]);
      }
      function St(P, U) {
        for (var $ = 0; $ |= 1 & P, P >>>= 1, $ <<= 1, 0 < --U; )
          ;
        return $ >>> 1;
      }
      function yt(P, U, $) {
        var Z, B, K = new Array(T + 1), st = 0;
        for (Z = 1; Z <= T; Z++)
          K[Z] = st = st + $[Z - 1] << 1;
        for (B = 0; B <= U; B++) {
          var J = P[2 * B + 1];
          J !== 0 && (P[2 * B] = St(K[J]++, J));
        }
      }
      function at(P) {
        var U;
        for (U = 0; U < I; U++)
          P.dyn_ltree[2 * U] = 0;
        for (U = 0; U < u; U++)
          P.dyn_dtree[2 * U] = 0;
        for (U = 0; U < d; U++)
          P.bl_tree[2 * U] = 0;
        P.dyn_ltree[2 * S] = 1, P.opt_len = P.static_len = 0, P.last_lit = P.matches = 0;
      }
      function It(P) {
        8 < P.bi_valid ? ot(P, P.bi_buf) : 0 < P.bi_valid && (P.pending_buf[P.pending++] = P.bi_buf), P.bi_buf = 0, P.bi_valid = 0;
      }
      function ft(P, U, $, Z) {
        var B = 2 * U, K = 2 * $;
        return P[B] < P[K] || P[B] === P[K] && Z[U] <= Z[$];
      }
      function Ct(P, U, $) {
        for (var Z = P.heap[$], B = $ << 1; B <= P.heap_len && (B < P.heap_len && ft(U, P.heap[B + 1], P.heap[B], P.depth) && B++, !ft(U, Z, P.heap[B], P.depth)); )
          P.heap[$] = P.heap[B], $ = B, B <<= 1;
        P.heap[$] = Z;
      }
      function Ht(P, U, $) {
        var Z, B, K, st, J = 0;
        if (P.last_lit !== 0)
          for (; Z = P.pending_buf[P.d_buf + 2 * J] << 8 | P.pending_buf[P.d_buf + 2 * J + 1], B = P.pending_buf[P.l_buf + J], J++, Z === 0 ? tt(P, B, U) : (tt(P, (K = g[B]) + f + 1, U), (st = y[K]) !== 0 && it(P, B -= v[K], st), tt(P, K = x(--Z), $), (st = w[K]) !== 0 && it(P, Z -= W[K], st)), J < P.last_lit; )
            ;
        tt(P, S, U);
      }
      function Dt(P, U) {
        var $, Z, B, K = U.dyn_tree, st = U.stat_desc.static_tree, J = U.stat_desc.has_stree, ct = U.stat_desc.elems, Rt = -1;
        for (P.heap_len = 0, P.heap_max = E, $ = 0; $ < ct; $++)
          K[2 * $] !== 0 ? (P.heap[++P.heap_len] = Rt = $, P.depth[$] = 0) : K[2 * $ + 1] = 0;
        for (; P.heap_len < 2; )
          K[2 * (B = P.heap[++P.heap_len] = Rt < 2 ? ++Rt : 0)] = 1, P.depth[B] = 0, P.opt_len--, J && (P.static_len -= st[2 * B + 1]);
        for (U.max_code = Rt, $ = P.heap_len >> 1; 1 <= $; $--)
          Ct(P, K, $);
        for (B = ct; $ = P.heap[1], P.heap[1] = P.heap[P.heap_len--], Ct(P, K, 1), Z = P.heap[1], P.heap[--P.heap_max] = $, P.heap[--P.heap_max] = Z, K[2 * B] = K[2 * $] + K[2 * Z], P.depth[B] = (P.depth[$] >= P.depth[Z] ? P.depth[$] : P.depth[Z]) + 1, K[2 * $ + 1] = K[2 * Z + 1] = B, P.heap[1] = B++, Ct(P, K, 1), 2 <= P.heap_len; )
          ;
        P.heap[--P.heap_max] = P.heap[1], function(dt, Wt) {
          var hi, ce, ui, Lt, Vi, Hs, Ce = Wt.dyn_tree, Xn = Wt.max_code, Ro = Wt.stat_desc.static_tree, go = Wt.stat_desc.has_stree, Ao = Wt.stat_desc.extra_bits, $n = Wt.stat_desc.extra_base, fi = Wt.stat_desc.max_length, Yi = 0;
          for (Lt = 0; Lt <= T; Lt++)
            dt.bl_count[Lt] = 0;
          for (Ce[2 * dt.heap[dt.heap_max] + 1] = 0, hi = dt.heap_max + 1; hi < E; hi++)
            fi < (Lt = Ce[2 * Ce[2 * (ce = dt.heap[hi]) + 1] + 1] + 1) && (Lt = fi, Yi++), Ce[2 * ce + 1] = Lt, Xn < ce || (dt.bl_count[Lt]++, Vi = 0, $n <= ce && (Vi = Ao[ce - $n]), Hs = Ce[2 * ce], dt.opt_len += Hs * (Lt + Vi), go && (dt.static_len += Hs * (Ro[2 * ce + 1] + Vi)));
          if (Yi !== 0) {
            do {
              for (Lt = fi - 1; dt.bl_count[Lt] === 0; )
                Lt--;
              dt.bl_count[Lt]--, dt.bl_count[Lt + 1] += 2, dt.bl_count[fi]--, Yi -= 2;
            } while (0 < Yi);
            for (Lt = fi; Lt !== 0; Lt--)
              for (ce = dt.bl_count[Lt]; ce !== 0; )
                Xn < (ui = dt.heap[--hi]) || (Ce[2 * ui + 1] !== Lt && (dt.opt_len += (Lt - Ce[2 * ui + 1]) * Ce[2 * ui], Ce[2 * ui + 1] = Lt), ce--);
          }
        }(P, U), yt(K, Rt, P.bl_count);
      }
      function A(P, U, $) {
        var Z, B, K = -1, st = U[1], J = 0, ct = 7, Rt = 4;
        for (st === 0 && (ct = 138, Rt = 3), U[2 * ($ + 1) + 1] = 65535, Z = 0; Z <= $; Z++)
          B = st, st = U[2 * (Z + 1) + 1], ++J < ct && B === st || (J < Rt ? P.bl_tree[2 * B] += J : B !== 0 ? (B !== K && P.bl_tree[2 * B]++, P.bl_tree[2 * m]++) : J <= 10 ? P.bl_tree[2 * F]++ : P.bl_tree[2 * O]++, K = B, Rt = (J = 0) === st ? (ct = 138, 3) : B === st ? (ct = 6, 3) : (ct = 7, 4));
      }
      function X(P, U, $) {
        var Z, B, K = -1, st = U[1], J = 0, ct = 7, Rt = 4;
        for (st === 0 && (ct = 138, Rt = 3), Z = 0; Z <= $; Z++)
          if (B = st, st = U[2 * (Z + 1) + 1], !(++J < ct && B === st)) {
            if (J < Rt)
              for (; tt(P, B, P.bl_tree), --J != 0; )
                ;
            else
              B !== 0 ? (B !== K && (tt(P, B, P.bl_tree), J--), tt(P, m, P.bl_tree), it(P, J - 3, 2)) : J <= 10 ? (tt(P, F, P.bl_tree), it(P, J - 3, 3)) : (tt(P, O, P.bl_tree), it(P, J - 11, 7));
            K = B, Rt = (J = 0) === st ? (ct = 138, 3) : B === st ? (ct = 6, 3) : (ct = 7, 4);
          }
      }
      a(W);
      var z = !1;
      function _(P, U, $, Z) {
        it(P, (l << 1) + (Z ? 1 : 0), 3), function(B, K, st, J) {
          It(B), ot(B, st), ot(B, ~st), n.arraySet(B.pending_buf, B.window, K, st, B.pending), B.pending += st;
        }(P, U, $);
      }
      s._tr_init = function(P) {
        z || (function() {
          var U, $, Z, B, K, st = new Array(T + 1);
          for (B = Z = 0; B < h - 1; B++)
            for (v[B] = Z, U = 0; U < 1 << y[B]; U++)
              g[Z++] = B;
          for (g[Z - 1] = B, B = K = 0; B < 16; B++)
            for (W[B] = K, U = 0; U < 1 << w[B]; U++)
              M[K++] = B;
          for (K >>= 7; B < u; B++)
            for (W[B] = K << 7, U = 0; U < 1 << w[B] - 7; U++)
              M[256 + K++] = B;
          for ($ = 0; $ <= T; $++)
            st[$] = 0;
          for (U = 0; U <= 143; )
            Y[2 * U + 1] = 8, U++, st[8]++;
          for (; U <= 255; )
            Y[2 * U + 1] = 9, U++, st[9]++;
          for (; U <= 279; )
            Y[2 * U + 1] = 7, U++, st[7]++;
          for (; U <= 287; )
            Y[2 * U + 1] = 8, U++, st[8]++;
          for (yt(Y, I + 1, st), U = 0; U < u; U++)
            N[2 * U + 1] = 5, N[2 * U] = St(U, 5);
          q = new nt(Y, y, f + 1, I, T), G = new nt(N, w, 0, u, T), et = new nt(new Array(0), L, 0, d, R);
        }(), z = !0), P.l_desc = new V(P.dyn_ltree, q), P.d_desc = new V(P.dyn_dtree, G), P.bl_desc = new V(P.bl_tree, et), P.bi_buf = 0, P.bi_valid = 0, at(P);
      }, s._tr_stored_block = _, s._tr_flush_block = function(P, U, $, Z) {
        var B, K, st = 0;
        0 < P.level ? (P.strm.data_type === 2 && (P.strm.data_type = function(J) {
          var ct, Rt = 4093624447;
          for (ct = 0; ct <= 31; ct++, Rt >>>= 1)
            if (1 & Rt && J.dyn_ltree[2 * ct] !== 0)
              return r;
          if (J.dyn_ltree[18] !== 0 || J.dyn_ltree[20] !== 0 || J.dyn_ltree[26] !== 0)
            return o;
          for (ct = 32; ct < f; ct++)
            if (J.dyn_ltree[2 * ct] !== 0)
              return o;
          return r;
        }(P)), Dt(P, P.l_desc), Dt(P, P.d_desc), st = function(J) {
          var ct;
          for (A(J, J.dyn_ltree, J.l_desc.max_code), A(J, J.dyn_dtree, J.d_desc.max_code), Dt(J, J.bl_desc), ct = d - 1; 3 <= ct && J.bl_tree[2 * b[ct] + 1] === 0; ct--)
            ;
          return J.opt_len += 3 * (ct + 1) + 5 + 5 + 4, ct;
        }(P), B = P.opt_len + 3 + 7 >>> 3, (K = P.static_len + 3 + 7 >>> 3) <= B && (B = K)) : B = K = $ + 5, $ + 4 <= B && U !== -1 ? _(P, U, $, Z) : P.strategy === 4 || K === B ? (it(P, 2 + (Z ? 1 : 0), 3), Ht(P, Y, N)) : (it(P, 4 + (Z ? 1 : 0), 3), function(J, ct, Rt, dt) {
          var Wt;
          for (it(J, ct - 257, 5), it(J, Rt - 1, 5), it(J, dt - 4, 4), Wt = 0; Wt < dt; Wt++)
            it(J, J.bl_tree[2 * b[Wt] + 1], 3);
          X(J, J.dyn_ltree, ct - 1), X(J, J.dyn_dtree, Rt - 1);
        }(P, P.l_desc.max_code + 1, P.d_desc.max_code + 1, st + 1), Ht(P, P.dyn_ltree, P.dyn_dtree)), at(P), Z && It(P);
      }, s._tr_tally = function(P, U, $) {
        return P.pending_buf[P.d_buf + 2 * P.last_lit] = U >>> 8 & 255, P.pending_buf[P.d_buf + 2 * P.last_lit + 1] = 255 & U, P.pending_buf[P.l_buf + P.last_lit] = 255 & $, P.last_lit++, U === 0 ? P.dyn_ltree[2 * $]++ : (P.matches++, U--, P.dyn_ltree[2 * (g[$] + f + 1)]++, P.dyn_dtree[2 * x(U)]++), P.last_lit === P.lit_bufsize - 1;
      }, s._tr_align = function(P) {
        it(P, 2, 3), tt(P, S, Y), function(U) {
          U.bi_valid === 16 ? (ot(U, U.bi_buf), U.bi_buf = 0, U.bi_valid = 0) : 8 <= U.bi_valid && (U.pending_buf[U.pending++] = 255 & U.bi_buf, U.bi_buf >>= 8, U.bi_valid -= 8);
        }(P);
      };
    }, { "../utils/common": 41 }], 53: [function(t, e, s) {
      e.exports = function() {
        this.input = null, this.next_in = 0, this.avail_in = 0, this.total_in = 0, this.output = null, this.next_out = 0, this.avail_out = 0, this.total_out = 0, this.msg = "", this.state = null, this.data_type = 2, this.adler = 0;
      };
    }, {}], 54: [function(t, e, s) {
      (function(n) {
        (function(r, o) {
          if (!r.setImmediate) {
            var a, l, h, f, I = 1, u = {}, d = !1, E = r.document, T = Object.getPrototypeOf && Object.getPrototypeOf(r);
            T = T && T.setTimeout ? T : r, a = {}.toString.call(r.process) === "[object process]" ? function(m) {
              process.nextTick(function() {
                R(m);
              });
            } : function() {
              if (r.postMessage && !r.importScripts) {
                var m = !0, F = r.onmessage;
                return r.onmessage = function() {
                  m = !1;
                }, r.postMessage("", "*"), r.onmessage = F, m;
              }
            }() ? (f = "setImmediate$" + Math.random() + "$", r.addEventListener ? r.addEventListener("message", S, !1) : r.attachEvent("onmessage", S), function(m) {
              r.postMessage(f + m, "*");
            }) : r.MessageChannel ? ((h = new MessageChannel()).port1.onmessage = function(m) {
              R(m.data);
            }, function(m) {
              h.port2.postMessage(m);
            }) : E && "onreadystatechange" in E.createElement("script") ? (l = E.documentElement, function(m) {
              var F = E.createElement("script");
              F.onreadystatechange = function() {
                R(m), F.onreadystatechange = null, l.removeChild(F), F = null;
              }, l.appendChild(F);
            }) : function(m) {
              setTimeout(R, 0, m);
            }, T.setImmediate = function(m) {
              typeof m != "function" && (m = new Function("" + m));
              for (var F = new Array(arguments.length - 1), O = 0; O < F.length; O++)
                F[O] = arguments[O + 1];
              var y = { callback: m, args: F };
              return u[I] = y, a(I), I++;
            }, T.clearImmediate = p;
          }
          function p(m) {
            delete u[m];
          }
          function R(m) {
            if (d)
              setTimeout(R, 0, m);
            else {
              var F = u[m];
              if (F) {
                d = !0;
                try {
                  (function(O) {
                    var y = O.callback, w = O.args;
                    switch (w.length) {
                      case 0:
                        y();
                        break;
                      case 1:
                        y(w[0]);
                        break;
                      case 2:
                        y(w[0], w[1]);
                        break;
                      case 3:
                        y(w[0], w[1], w[2]);
                        break;
                      default:
                        y.apply(o, w);
                    }
                  })(F);
                } finally {
                  p(m), d = !1;
                }
              }
            }
          }
          function S(m) {
            m.source === r && typeof m.data == "string" && m.data.indexOf(f) === 0 && R(+m.data.slice(f.length));
          }
        })(typeof self > "u" ? n === void 0 ? this : n : self);
      }).call(this, typeof us < "u" ? us : typeof self < "u" ? self : typeof window < "u" ? window : {});
    }, {}] }, {}, [10])(10);
  });
})(ho);
var Uc = ho.exports;
const Ur = /* @__PURE__ */ vc(Uc);
var kn = {}, zs = {};
(function(c) {
  const i = ":A-Za-z_\\u00C0-\\u00D6\\u00D8-\\u00F6\\u00F8-\\u02FF\\u0370-\\u037D\\u037F-\\u1FFF\\u200C-\\u200D\\u2070-\\u218F\\u2C00-\\u2FEF\\u3001-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFFD", t = i + "\\-.\\d\\u00B7\\u0300-\\u036F\\u203F-\\u2040", e = "[" + i + "][" + t + "]*", s = new RegExp("^" + e + "$"), n = function(o, a) {
    const l = [];
    let h = a.exec(o);
    for (; h; ) {
      const f = [];
      f.startIndex = a.lastIndex - h[0].length;
      const I = h.length;
      for (let u = 0; u < I; u++)
        f.push(h[u]);
      l.push(f), h = a.exec(o);
    }
    return l;
  }, r = function(o) {
    const a = s.exec(o);
    return !(a === null || typeof a > "u");
  };
  c.isExist = function(o) {
    return typeof o < "u";
  }, c.isEmptyObject = function(o) {
    return Object.keys(o).length === 0;
  }, c.merge = function(o, a, l) {
    if (a) {
      const h = Object.keys(a), f = h.length;
      for (let I = 0; I < f; I++)
        l === "strict" ? o[h[I]] = [a[h[I]]] : o[h[I]] = a[h[I]];
    }
  }, c.getValue = function(o) {
    return c.isExist(o) ? o : "";
  }, c.isName = r, c.getAllMatches = n, c.nameRegexp = e;
})(zs);
const Hn = zs, xc = {
  allowBooleanAttributes: !1,
  //A tag can have attributes without any value
  unpairedTags: []
};
kn.validate = function(c, i) {
  i = Object.assign({}, xc, i);
  const t = [];
  let e = !1, s = !1;
  c[0] === "\uFEFF" && (c = c.substr(1));
  for (let n = 0; n < c.length; n++)
    if (c[n] === "<" && c[n + 1] === "?") {
      if (n += 2, n = Br(c, n), n.err)
        return n;
    } else if (c[n] === "<") {
      let r = n;
      if (n++, c[n] === "!") {
        n = Vr(c, n);
        continue;
      } else {
        let o = !1;
        c[n] === "/" && (o = !0, n++);
        let a = "";
        for (; n < c.length && c[n] !== ">" && c[n] !== " " && c[n] !== "	" && c[n] !== `
` && c[n] !== "\r"; n++)
          a += c[n];
        if (a = a.trim(), a[a.length - 1] === "/" && (a = a.substring(0, a.length - 1), n--), !Wc(a)) {
          let f;
          return a.trim().length === 0 ? f = "Invalid space after '<'." : f = "Tag '" + a + "' is an invalid name.", _t("InvalidTag", f, xt(c, n));
        }
        const l = Yc(c, n);
        if (l === !1)
          return _t("InvalidAttr", "Attributes for '" + a + "' have open quote.", xt(c, n));
        let h = l.value;
        if (n = l.index, h[h.length - 1] === "/") {
          const f = n - h.length;
          h = h.substring(0, h.length - 1);
          const I = Yr(h, i);
          if (I === !0)
            e = !0;
          else
            return _t(I.err.code, I.err.msg, xt(c, f + I.err.line));
        } else if (o)
          if (l.tagClosed) {
            if (h.trim().length > 0)
              return _t("InvalidTag", "Closing tag '" + a + "' can't have attributes or invalid starting.", xt(c, r));
            if (t.length === 0)
              return _t("InvalidTag", "Closing tag '" + a + "' has not been opened.", xt(c, r));
            {
              const f = t.pop();
              if (a !== f.tagName) {
                let I = xt(c, f.tagStartPos);
                return _t(
                  "InvalidTag",
                  "Expected closing tag '" + f.tagName + "' (opened in line " + I.line + ", col " + I.col + ") instead of closing tag '" + a + "'.",
                  xt(c, r)
                );
              }
              t.length == 0 && (s = !0);
            }
          } else
            return _t("InvalidTag", "Closing tag '" + a + "' doesn't have proper closing.", xt(c, n));
        else {
          const f = Yr(h, i);
          if (f !== !0)
            return _t(f.err.code, f.err.msg, xt(c, n - h.length + f.err.line));
          if (s === !0)
            return _t("InvalidXml", "Multiple possible root nodes found.", xt(c, n));
          i.unpairedTags.indexOf(a) !== -1 || t.push({ tagName: a, tagStartPos: r }), e = !0;
        }
        for (n++; n < c.length; n++)
          if (c[n] === "<")
            if (c[n + 1] === "!") {
              n++, n = Vr(c, n);
              continue;
            } else if (c[n + 1] === "?") {
              if (n = Br(c, ++n), n.err)
                return n;
            } else
              break;
          else if (c[n] === "&") {
            const f = kc(c, n);
            if (f == -1)
              return _t("InvalidChar", "char '&' is not expected.", xt(c, n));
            n = f;
          } else if (s === !0 && !xr(c[n]))
            return _t("InvalidXml", "Extra text at the end", xt(c, n));
        c[n] === "<" && n--;
      }
    } else {
      if (xr(c[n]))
        continue;
      return _t("InvalidChar", "char '" + c[n] + "' is not expected.", xt(c, n));
    }
  if (e) {
    if (t.length == 1)
      return _t("InvalidTag", "Unclosed tag '" + t[0].tagName + "'.", xt(c, t[0].tagStartPos));
    if (t.length > 0)
      return _t("InvalidXml", "Invalid '" + JSON.stringify(t.map((n) => n.tagName), null, 4).replace(/\r?\n/g, "") + "' found.", { line: 1, col: 1 });
  } else
    return _t("InvalidXml", "Start tag expected.", 1);
  return !0;
};
function xr(c) {
  return c === " " || c === "	" || c === `
` || c === "\r";
}
function Br(c, i) {
  const t = i;
  for (; i < c.length; i++)
    if (c[i] == "?" || c[i] == " ") {
      const e = c.substr(t, i - t);
      if (i > 5 && e === "xml")
        return _t("InvalidXml", "XML declaration allowed only at the start of the document.", xt(c, i));
      if (c[i] == "?" && c[i + 1] == ">") {
        i++;
        break;
      } else
        continue;
    }
  return i;
}
function Vr(c, i) {
  if (c.length > i + 5 && c[i + 1] === "-" && c[i + 2] === "-") {
    for (i += 3; i < c.length; i++)
      if (c[i] === "-" && c[i + 1] === "-" && c[i + 2] === ">") {
        i += 2;
        break;
      }
  } else if (c.length > i + 8 && c[i + 1] === "D" && c[i + 2] === "O" && c[i + 3] === "C" && c[i + 4] === "T" && c[i + 5] === "Y" && c[i + 6] === "P" && c[i + 7] === "E") {
    let t = 1;
    for (i += 8; i < c.length; i++)
      if (c[i] === "<")
        t++;
      else if (c[i] === ">" && (t--, t === 0))
        break;
  } else if (c.length > i + 9 && c[i + 1] === "[" && c[i + 2] === "C" && c[i + 3] === "D" && c[i + 4] === "A" && c[i + 5] === "T" && c[i + 6] === "A" && c[i + 7] === "[") {
    for (i += 8; i < c.length; i++)
      if (c[i] === "]" && c[i + 1] === "]" && c[i + 2] === ">") {
        i += 2;
        break;
      }
  }
  return i;
}
const Bc = '"', Vc = "'";
function Yc(c, i) {
  let t = "", e = "", s = !1;
  for (; i < c.length; i++) {
    if (c[i] === Bc || c[i] === Vc)
      e === "" ? e = c[i] : e !== c[i] || (e = "");
    else if (c[i] === ">" && e === "") {
      s = !0;
      break;
    }
    t += c[i];
  }
  return e !== "" ? !1 : {
    value: t,
    index: i,
    tagClosed: s
  };
}
const Gc = new RegExp(`(\\s*)([^\\s=]+)(\\s*=)?(\\s*(['"])(([\\s\\S])*?)\\5)?`, "g");
function Yr(c, i) {
  const t = Hn.getAllMatches(c, Gc), e = {};
  for (let s = 0; s < t.length; s++) {
    if (t[s][1].length === 0)
      return _t("InvalidAttr", "Attribute '" + t[s][2] + "' has no space in starting.", yi(t[s]));
    if (t[s][3] !== void 0 && t[s][4] === void 0)
      return _t("InvalidAttr", "Attribute '" + t[s][2] + "' is without value.", yi(t[s]));
    if (t[s][3] === void 0 && !i.allowBooleanAttributes)
      return _t("InvalidAttr", "boolean attribute '" + t[s][2] + "' is not allowed.", yi(t[s]));
    const n = t[s][2];
    if (!Hc(n))
      return _t("InvalidAttr", "Attribute '" + n + "' is an invalid name.", yi(t[s]));
    if (!e.hasOwnProperty(n))
      e[n] = 1;
    else
      return _t("InvalidAttr", "Attribute '" + n + "' is repeated.", yi(t[s]));
  }
  return !0;
}
function zc(c, i) {
  let t = /\d/;
  for (c[i] === "x" && (i++, t = /[\da-fA-F]/); i < c.length; i++) {
    if (c[i] === ";")
      return i;
    if (!c[i].match(t))
      break;
  }
  return -1;
}
function kc(c, i) {
  if (i++, c[i] === ";")
    return -1;
  if (c[i] === "#")
    return i++, zc(c, i);
  let t = 0;
  for (; i < c.length; i++, t++)
    if (!(c[i].match(/\w/) && t < 20)) {
      if (c[i] === ";")
        break;
      return -1;
    }
  return i;
}
function _t(c, i, t) {
  return {
    err: {
      code: c,
      msg: i,
      line: t.line || t,
      col: t.col
    }
  };
}
function Hc(c) {
  return Hn.isName(c);
}
function Wc(c) {
  return Hn.isName(c);
}
function xt(c, i) {
  const t = c.substring(0, i).split(/\r?\n/);
  return {
    line: t.length,
    // column number is last line's length + 1, because column numbering starts at 1:
    col: t[t.length - 1].length + 1
  };
}
function yi(c) {
  return c.startIndex + c[1].length;
}
var Wn = {};
const uo = {
  preserveOrder: !1,
  attributeNamePrefix: "@_",
  attributesGroupName: !1,
  textNodeName: "#text",
  ignoreAttributes: !0,
  removeNSPrefix: !1,
  // remove NS from tag name or attribute name if true
  allowBooleanAttributes: !1,
  //a tag can have attributes without any value
  //ignoreRootElement : false,
  parseTagValue: !0,
  parseAttributeValue: !1,
  trimValues: !0,
  //Trim string values of tag and attributes
  cdataPropName: !1,
  numberParseOptions: {
    hex: !0,
    leadingZeros: !0,
    eNotation: !0
  },
  tagValueProcessor: function(c, i) {
    return i;
  },
  attributeValueProcessor: function(c, i) {
    return i;
  },
  stopNodes: [],
  //nested tags will not be parsed even for errors
  alwaysCreateTextNode: !1,
  isArray: () => !1,
  commentPropName: !1,
  unpairedTags: [],
  processEntities: !0,
  htmlEntities: !1,
  ignoreDeclaration: !1,
  ignorePiTags: !1,
  transformTagName: !1,
  transformAttributeName: !1,
  updateTag: function(c, i, t) {
    return c;
  }
  // skipEmptyListItem: false
}, Xc = function(c) {
  return Object.assign({}, uo, c);
};
Wn.buildOptions = Xc;
Wn.defaultOptions = uo;
class $c {
  constructor(i) {
    this.tagname = i, this.child = [], this[":@"] = {};
  }
  add(i, t) {
    i === "__proto__" && (i = "#__proto__"), this.child.push({ [i]: t });
  }
  addChild(i) {
    i.tagname === "__proto__" && (i.tagname = "#__proto__"), i[":@"] && Object.keys(i[":@"]).length > 0 ? this.child.push({ [i.tagname]: i.child, ":@": i[":@"] }) : this.child.push({ [i.tagname]: i.child });
  }
}
var Zc = $c;
const jc = zs;
function qc(c, i) {
  const t = {};
  if (c[i + 3] === "O" && c[i + 4] === "C" && c[i + 5] === "T" && c[i + 6] === "Y" && c[i + 7] === "P" && c[i + 8] === "E") {
    i = i + 9;
    let e = 1, s = !1, n = !1, r = "";
    for (; i < c.length; i++)
      if (c[i] === "<" && !n) {
        if (s && Jc(c, i))
          i += 7, [entityName, val, i] = Qc(c, i + 1), val.indexOf("&") === -1 && (t[sl(entityName)] = {
            regx: RegExp(`&${entityName};`, "g"),
            val
          });
        else if (s && tl(c, i))
          i += 8;
        else if (s && el(c, i))
          i += 8;
        else if (s && il(c, i))
          i += 9;
        else if (Kc)
          n = !0;
        else
          throw new Error("Invalid DOCTYPE");
        e++, r = "";
      } else if (c[i] === ">") {
        if (n ? c[i - 1] === "-" && c[i - 2] === "-" && (n = !1, e--) : e--, e === 0)
          break;
      } else
        c[i] === "[" ? s = !0 : r += c[i];
    if (e !== 0)
      throw new Error("Unclosed DOCTYPE");
  } else
    throw new Error("Invalid Tag instead of DOCTYPE");
  return { entities: t, i };
}
function Qc(c, i) {
  let t = "";
  for (; i < c.length && c[i] !== "'" && c[i] !== '"'; i++)
    t += c[i];
  if (t = t.trim(), t.indexOf(" ") !== -1)
    throw new Error("External entites are not supported");
  const e = c[i++];
  let s = "";
  for (; i < c.length && c[i] !== e; i++)
    s += c[i];
  return [t, s, i];
}
function Kc(c, i) {
  return c[i + 1] === "!" && c[i + 2] === "-" && c[i + 3] === "-";
}
function Jc(c, i) {
  return c[i + 1] === "!" && c[i + 2] === "E" && c[i + 3] === "N" && c[i + 4] === "T" && c[i + 5] === "I" && c[i + 6] === "T" && c[i + 7] === "Y";
}
function tl(c, i) {
  return c[i + 1] === "!" && c[i + 2] === "E" && c[i + 3] === "L" && c[i + 4] === "E" && c[i + 5] === "M" && c[i + 6] === "E" && c[i + 7] === "N" && c[i + 8] === "T";
}
function el(c, i) {
  return c[i + 1] === "!" && c[i + 2] === "A" && c[i + 3] === "T" && c[i + 4] === "T" && c[i + 5] === "L" && c[i + 6] === "I" && c[i + 7] === "S" && c[i + 8] === "T";
}
function il(c, i) {
  return c[i + 1] === "!" && c[i + 2] === "N" && c[i + 3] === "O" && c[i + 4] === "T" && c[i + 5] === "A" && c[i + 6] === "T" && c[i + 7] === "I" && c[i + 8] === "O" && c[i + 9] === "N";
}
function sl(c) {
  if (jc.isName(c))
    return c;
  throw new Error(`Invalid entity name ${c}`);
}
var nl = qc;
const rl = /^[-+]?0x[a-fA-F0-9]+$/, ol = /^([\-\+])?(0*)(\.[0-9]+([eE]\-?[0-9]+)?|[0-9]+(\.[0-9]+([eE]\-?[0-9]+)?)?)$/;
!Number.parseInt && window.parseInt && (Number.parseInt = window.parseInt);
!Number.parseFloat && window.parseFloat && (Number.parseFloat = window.parseFloat);
const al = {
  hex: !0,
  leadingZeros: !0,
  decimalPoint: ".",
  eNotation: !0
  //skipLike: /regex/
};
function cl(c, i = {}) {
  if (i = Object.assign({}, al, i), !c || typeof c != "string")
    return c;
  let t = c.trim();
  if (i.skipLike !== void 0 && i.skipLike.test(t))
    return c;
  if (i.hex && rl.test(t))
    return Number.parseInt(t, 16);
  {
    const e = ol.exec(t);
    if (e) {
      const s = e[1], n = e[2];
      let r = ll(e[3]);
      const o = e[4] || e[6];
      if (!i.leadingZeros && n.length > 0 && s && t[2] !== ".")
        return c;
      if (!i.leadingZeros && n.length > 0 && !s && t[1] !== ".")
        return c;
      {
        const a = Number(t), l = "" + a;
        return l.search(/[eE]/) !== -1 || o ? i.eNotation ? a : c : t.indexOf(".") !== -1 ? l === "0" && r === "" || l === r || s && l === "-" + r ? a : c : n ? r === l || s + r === l ? a : c : t === l || t === s + l ? a : c;
      }
    } else
      return c;
  }
}
function ll(c) {
  return c && c.indexOf(".") !== -1 && (c = c.replace(/0+$/, ""), c === "." ? c = "0" : c[0] === "." ? c = "0" + c : c[c.length - 1] === "." && (c = c.substr(0, c.length - 1))), c;
}
var hl = cl;
const fo = zs, Li = Zc, ul = nl, fl = hl;
let Il = class {
  constructor(i) {
    this.options = i, this.currentNode = null, this.tagsNodeStack = [], this.docTypeEntities = {}, this.lastEntities = {
      apos: { regex: /&(apos|#39|#x27);/g, val: "'" },
      gt: { regex: /&(gt|#62|#x3E);/g, val: ">" },
      lt: { regex: /&(lt|#60|#x3C);/g, val: "<" },
      quot: { regex: /&(quot|#34|#x22);/g, val: '"' }
    }, this.ampEntity = { regex: /&(amp|#38|#x26);/g, val: "&" }, this.htmlEntities = {
      space: { regex: /&(nbsp|#160);/g, val: " " },
      // "lt" : { regex: /&(lt|#60);/g, val: "<" },
      // "gt" : { regex: /&(gt|#62);/g, val: ">" },
      // "amp" : { regex: /&(amp|#38);/g, val: "&" },
      // "quot" : { regex: /&(quot|#34);/g, val: "\"" },
      // "apos" : { regex: /&(apos|#39);/g, val: "'" },
      cent: { regex: /&(cent|#162);/g, val: "¢" },
      pound: { regex: /&(pound|#163);/g, val: "£" },
      yen: { regex: /&(yen|#165);/g, val: "¥" },
      euro: { regex: /&(euro|#8364);/g, val: "€" },
      copyright: { regex: /&(copy|#169);/g, val: "©" },
      reg: { regex: /&(reg|#174);/g, val: "®" },
      inr: { regex: /&(inr|#8377);/g, val: "₹" },
      num_dec: { regex: /&#([0-9]{1,7});/g, val: (t, e) => String.fromCharCode(Number.parseInt(e, 10)) },
      num_hex: { regex: /&#x([0-9a-fA-F]{1,6});/g, val: (t, e) => String.fromCharCode(Number.parseInt(e, 16)) }
    }, this.addExternalEntities = dl, this.parseXml = ml, this.parseTextData = El, this.resolveNameSpace = pl, this.buildAttributesMap = Tl, this.isItStopNode = Fl, this.replaceEntitiesValue = gl, this.readStopNodeData = Ol, this.saveTextToParentTag = Al, this.addChild = Rl;
  }
};
function dl(c) {
  const i = Object.keys(c);
  for (let t = 0; t < i.length; t++) {
    const e = i[t];
    this.lastEntities[e] = {
      regex: new RegExp("&" + e + ";", "g"),
      val: c[e]
    };
  }
}
function El(c, i, t, e, s, n, r) {
  if (c !== void 0 && (this.options.trimValues && !e && (c = c.trim()), c.length > 0)) {
    r || (c = this.replaceEntitiesValue(c));
    const o = this.options.tagValueProcessor(i, c, t, s, n);
    return o == null ? c : typeof o != typeof c || o !== c ? o : this.options.trimValues ? _n(c, this.options.parseTagValue, this.options.numberParseOptions) : c.trim() === c ? _n(c, this.options.parseTagValue, this.options.numberParseOptions) : c;
  }
}
function pl(c) {
  if (this.options.removeNSPrefix) {
    const i = c.split(":"), t = c.charAt(0) === "/" ? "/" : "";
    if (i[0] === "xmlns")
      return "";
    i.length === 2 && (c = t + i[1]);
  }
  return c;
}
const Cl = new RegExp(`([^\\s=]+)\\s*(=\\s*(['"])([\\s\\S]*?)\\3)?`, "gm");
function Tl(c, i, t) {
  if (!this.options.ignoreAttributes && typeof c == "string") {
    const e = fo.getAllMatches(c, Cl), s = e.length, n = {};
    for (let r = 0; r < s; r++) {
      const o = this.resolveNameSpace(e[r][1]);
      let a = e[r][4], l = this.options.attributeNamePrefix + o;
      if (o.length)
        if (this.options.transformAttributeName && (l = this.options.transformAttributeName(l)), l === "__proto__" && (l = "#__proto__"), a !== void 0) {
          this.options.trimValues && (a = a.trim()), a = this.replaceEntitiesValue(a);
          const h = this.options.attributeValueProcessor(o, a, i);
          h == null ? n[l] = a : typeof h != typeof a || h !== a ? n[l] = h : n[l] = _n(
            a,
            this.options.parseAttributeValue,
            this.options.numberParseOptions
          );
        } else
          this.options.allowBooleanAttributes && (n[l] = !0);
    }
    if (!Object.keys(n).length)
      return;
    if (this.options.attributesGroupName) {
      const r = {};
      return r[this.options.attributesGroupName] = n, r;
    }
    return n;
  }
}
const ml = function(c) {
  c = c.replace(/\r\n?/g, `
`);
  const i = new Li("!xml");
  let t = i, e = "", s = "";
  for (let n = 0; n < c.length; n++)
    if (c[n] === "<")
      if (c[n + 1] === "/") {
        const o = Ve(c, ">", n, "Closing Tag is not closed.");
        let a = c.substring(n + 2, o).trim();
        if (this.options.removeNSPrefix) {
          const f = a.indexOf(":");
          f !== -1 && (a = a.substr(f + 1));
        }
        this.options.transformTagName && (a = this.options.transformTagName(a)), t && (e = this.saveTextToParentTag(e, t, s));
        const l = s.substring(s.lastIndexOf(".") + 1);
        if (a && this.options.unpairedTags.indexOf(a) !== -1)
          throw new Error(`Unpaired tag can not be used as closing tag: </${a}>`);
        let h = 0;
        l && this.options.unpairedTags.indexOf(l) !== -1 ? (h = s.lastIndexOf(".", s.lastIndexOf(".") - 1), this.tagsNodeStack.pop()) : h = s.lastIndexOf("."), s = s.substring(0, h), t = this.tagsNodeStack.pop(), e = "", n = o;
      } else if (c[n + 1] === "?") {
        let o = Pn(c, n, !1, "?>");
        if (!o)
          throw new Error("Pi Tag is not closed.");
        if (e = this.saveTextToParentTag(e, t, s), !(this.options.ignoreDeclaration && o.tagName === "?xml" || this.options.ignorePiTags)) {
          const a = new Li(o.tagName);
          a.add(this.options.textNodeName, ""), o.tagName !== o.tagExp && o.attrExpPresent && (a[":@"] = this.buildAttributesMap(o.tagExp, s, o.tagName)), this.addChild(t, a, s);
        }
        n = o.closeIndex + 1;
      } else if (c.substr(n + 1, 3) === "!--") {
        const o = Ve(c, "-->", n + 4, "Comment is not closed.");
        if (this.options.commentPropName) {
          const a = c.substring(n + 4, o - 2);
          e = this.saveTextToParentTag(e, t, s), t.add(this.options.commentPropName, [{ [this.options.textNodeName]: a }]);
        }
        n = o;
      } else if (c.substr(n + 1, 2) === "!D") {
        const o = ul(c, n);
        this.docTypeEntities = o.entities, n = o.i;
      } else if (c.substr(n + 1, 2) === "![") {
        const o = Ve(c, "]]>", n, "CDATA is not closed.") - 2, a = c.substring(n + 9, o);
        e = this.saveTextToParentTag(e, t, s);
        let l = this.parseTextData(a, t.tagname, s, !0, !1, !0, !0);
        l == null && (l = ""), this.options.cdataPropName ? t.add(this.options.cdataPropName, [{ [this.options.textNodeName]: a }]) : t.add(this.options.textNodeName, l), n = o + 2;
      } else {
        let o = Pn(c, n, this.options.removeNSPrefix), a = o.tagName;
        const l = o.rawTagName;
        let h = o.tagExp, f = o.attrExpPresent, I = o.closeIndex;
        this.options.transformTagName && (a = this.options.transformTagName(a)), t && e && t.tagname !== "!xml" && (e = this.saveTextToParentTag(e, t, s, !1));
        const u = t;
        if (u && this.options.unpairedTags.indexOf(u.tagname) !== -1 && (t = this.tagsNodeStack.pop(), s = s.substring(0, s.lastIndexOf("."))), a !== i.tagname && (s += s ? "." + a : a), this.isItStopNode(this.options.stopNodes, s, a)) {
          let d = "";
          if (h.length > 0 && h.lastIndexOf("/") === h.length - 1)
            a[a.length - 1] === "/" ? (a = a.substr(0, a.length - 1), s = s.substr(0, s.length - 1), h = a) : h = h.substr(0, h.length - 1), n = o.closeIndex;
          else if (this.options.unpairedTags.indexOf(a) !== -1)
            n = o.closeIndex;
          else {
            const T = this.readStopNodeData(c, l, I + 1);
            if (!T)
              throw new Error(`Unexpected end of ${l}`);
            n = T.i, d = T.tagContent;
          }
          const E = new Li(a);
          a !== h && f && (E[":@"] = this.buildAttributesMap(h, s, a)), d && (d = this.parseTextData(d, a, s, !0, f, !0, !0)), s = s.substr(0, s.lastIndexOf(".")), E.add(this.options.textNodeName, d), this.addChild(t, E, s);
        } else {
          if (h.length > 0 && h.lastIndexOf("/") === h.length - 1) {
            a[a.length - 1] === "/" ? (a = a.substr(0, a.length - 1), s = s.substr(0, s.length - 1), h = a) : h = h.substr(0, h.length - 1), this.options.transformTagName && (a = this.options.transformTagName(a));
            const d = new Li(a);
            a !== h && f && (d[":@"] = this.buildAttributesMap(h, s, a)), this.addChild(t, d, s), s = s.substr(0, s.lastIndexOf("."));
          } else {
            const d = new Li(a);
            this.tagsNodeStack.push(t), a !== h && f && (d[":@"] = this.buildAttributesMap(h, s, a)), this.addChild(t, d, s), t = d;
          }
          e = "", n = I;
        }
      }
    else
      e += c[n];
  return i.child;
};
function Rl(c, i, t) {
  const e = this.options.updateTag(i.tagname, t, i[":@"]);
  e === !1 || (typeof e == "string" && (i.tagname = e), c.addChild(i));
}
const gl = function(c) {
  if (this.options.processEntities) {
    for (let i in this.docTypeEntities) {
      const t = this.docTypeEntities[i];
      c = c.replace(t.regx, t.val);
    }
    for (let i in this.lastEntities) {
      const t = this.lastEntities[i];
      c = c.replace(t.regex, t.val);
    }
    if (this.options.htmlEntities)
      for (let i in this.htmlEntities) {
        const t = this.htmlEntities[i];
        c = c.replace(t.regex, t.val);
      }
    c = c.replace(this.ampEntity.regex, this.ampEntity.val);
  }
  return c;
};
function Al(c, i, t, e) {
  return c && (e === void 0 && (e = Object.keys(i.child).length === 0), c = this.parseTextData(
    c,
    i.tagname,
    t,
    !1,
    i[":@"] ? Object.keys(i[":@"]).length !== 0 : !1,
    e
  ), c !== void 0 && c !== "" && i.add(this.options.textNodeName, c), c = ""), c;
}
function Fl(c, i, t) {
  const e = "*." + t;
  for (const s in c) {
    const n = c[s];
    if (e === n || i === n)
      return !0;
  }
  return !1;
}
function Sl(c, i, t = ">") {
  let e, s = "";
  for (let n = i; n < c.length; n++) {
    let r = c[n];
    if (e)
      r === e && (e = "");
    else if (r === '"' || r === "'")
      e = r;
    else if (r === t[0])
      if (t[1]) {
        if (c[n + 1] === t[1])
          return {
            data: s,
            index: n
          };
      } else
        return {
          data: s,
          index: n
        };
    else
      r === "	" && (r = " ");
    s += r;
  }
}
function Ve(c, i, t, e) {
  const s = c.indexOf(i, t);
  if (s === -1)
    throw new Error(e);
  return s + i.length - 1;
}
function Pn(c, i, t, e = ">") {
  const s = Sl(c, i + 1, e);
  if (!s)
    return;
  let n = s.data;
  const r = s.index, o = n.search(/\s/);
  let a = n, l = !0;
  o !== -1 && (a = n.substring(0, o), n = n.substring(o + 1).trimStart());
  const h = a;
  if (t) {
    const f = a.indexOf(":");
    f !== -1 && (a = a.substr(f + 1), l = a !== s.data.substr(f + 1));
  }
  return {
    tagName: a,
    tagExp: n,
    closeIndex: r,
    attrExpPresent: l,
    rawTagName: h
  };
}
function Ol(c, i, t) {
  const e = t;
  let s = 1;
  for (; t < c.length; t++)
    if (c[t] === "<")
      if (c[t + 1] === "/") {
        const n = Ve(c, ">", t, `${i} is not closed`);
        if (c.substring(t + 2, n).trim() === i && (s--, s === 0))
          return {
            tagContent: c.substring(e, t),
            i: n
          };
        t = n;
      } else if (c[t + 1] === "?")
        t = Ve(c, "?>", t + 1, "StopNode is not closed.");
      else if (c.substr(t + 1, 3) === "!--")
        t = Ve(c, "-->", t + 3, "StopNode is not closed.");
      else if (c.substr(t + 1, 2) === "![")
        t = Ve(c, "]]>", t, "StopNode is not closed.") - 2;
      else {
        const n = Pn(c, t, ">");
        n && ((n && n.tagName) === i && n.tagExp[n.tagExp.length - 1] !== "/" && s++, t = n.closeIndex);
      }
}
function _n(c, i, t) {
  if (i && typeof c == "string") {
    const e = c.trim();
    return e === "true" ? !0 : e === "false" ? !1 : fl(c, t);
  } else
    return fo.isExist(c) ? c : "";
}
var Nl = Il, Io = {};
function yl(c, i) {
  return Eo(c, i);
}
function Eo(c, i, t) {
  let e;
  const s = {};
  for (let n = 0; n < c.length; n++) {
    const r = c[n], o = Ll(r);
    let a = "";
    if (t === void 0 ? a = o : a = t + "." + o, o === i.textNodeName)
      e === void 0 ? e = r[o] : e += "" + r[o];
    else {
      if (o === void 0)
        continue;
      if (r[o]) {
        let l = Eo(r[o], i, a);
        const h = _l(l, i);
        r[":@"] ? Pl(l, r[":@"], a, i) : Object.keys(l).length === 1 && l[i.textNodeName] !== void 0 && !i.alwaysCreateTextNode ? l = l[i.textNodeName] : Object.keys(l).length === 0 && (i.alwaysCreateTextNode ? l[i.textNodeName] = "" : l = ""), s[o] !== void 0 && s.hasOwnProperty(o) ? (Array.isArray(s[o]) || (s[o] = [s[o]]), s[o].push(l)) : i.isArray(o, a, h) ? s[o] = [l] : s[o] = l;
      }
    }
  }
  return typeof e == "string" ? e.length > 0 && (s[i.textNodeName] = e) : e !== void 0 && (s[i.textNodeName] = e), s;
}
function Ll(c) {
  const i = Object.keys(c);
  for (let t = 0; t < i.length; t++) {
    const e = i[t];
    if (e !== ":@")
      return e;
  }
}
function Pl(c, i, t, e) {
  if (i) {
    const s = Object.keys(i), n = s.length;
    for (let r = 0; r < n; r++) {
      const o = s[r];
      e.isArray(o, t + "." + o, !0, !0) ? c[o] = [i[o]] : c[o] = i[o];
    }
  }
}
function _l(c, i) {
  const { textNodeName: t } = i, e = Object.keys(c).length;
  return !!(e === 0 || e === 1 && (c[t] || typeof c[t] == "boolean" || c[t] === 0));
}
Io.prettify = yl;
const { buildOptions: wl } = Wn, Ml = Nl, { prettify: Dl } = Io, bl = kn;
let vl = class {
  constructor(i) {
    this.externalEntities = {}, this.options = wl(i);
  }
  /**
   * Parse XML dats to JS object 
   * @param {string|Buffer} xmlData 
   * @param {boolean|Object} validationOption 
   */
  parse(i, t) {
    if (typeof i != "string")
      if (i.toString)
        i = i.toString();
      else
        throw new Error("XML data is accepted in String or Bytes[] form.");
    if (t) {
      t === !0 && (t = {});
      const n = bl.validate(i, t);
      if (n !== !0)
        throw Error(`${n.err.msg}:${n.err.line}:${n.err.col}`);
    }
    const e = new Ml(this.options);
    e.addExternalEntities(this.externalEntities);
    const s = e.parseXml(i);
    return this.options.preserveOrder || s === void 0 ? s : Dl(s, this.options);
  }
  /**
   * Add Entity which is not by default supported by this library
   * @param {string} key 
   * @param {string} value 
   */
  addEntity(i, t) {
    if (t.indexOf("&") !== -1)
      throw new Error("Entity value can't have '&'");
    if (i.indexOf("&") !== -1 || i.indexOf(";") !== -1)
      throw new Error("An entity must be set without '&' and ';'. Eg. use '#xD' for '&#xD;'");
    if (t === "&")
      throw new Error("An entity with value '&' is not permitted");
    this.externalEntities[i] = t;
  }
};
var Ul = vl;
const xl = `
`;
function Bl(c, i) {
  let t = "";
  return i.format && i.indentBy.length > 0 && (t = xl), po(c, i, "", t);
}
function po(c, i, t, e) {
  let s = "", n = !1;
  for (let r = 0; r < c.length; r++) {
    const o = c[r], a = Vl(o);
    if (a === void 0)
      continue;
    let l = "";
    if (t.length === 0 ? l = a : l = `${t}.${a}`, a === i.textNodeName) {
      let d = o[a];
      Yl(l, i) || (d = i.tagValueProcessor(a, d), d = Co(d, i)), n && (s += e), s += d, n = !1;
      continue;
    } else if (a === i.cdataPropName) {
      n && (s += e), s += `<![CDATA[${o[a][0][i.textNodeName]}]]>`, n = !1;
      continue;
    } else if (a === i.commentPropName) {
      s += e + `<!--${o[a][0][i.textNodeName]}-->`, n = !0;
      continue;
    } else if (a[0] === "?") {
      const d = Gr(o[":@"], i), E = a === "?xml" ? "" : e;
      let T = o[a][0][i.textNodeName];
      T = T.length !== 0 ? " " + T : "", s += E + `<${a}${T}${d}?>`, n = !0;
      continue;
    }
    let h = e;
    h !== "" && (h += i.indentBy);
    const f = Gr(o[":@"], i), I = e + `<${a}${f}`, u = po(o[a], i, l, h);
    i.unpairedTags.indexOf(a) !== -1 ? i.suppressUnpairedNode ? s += I + ">" : s += I + "/>" : (!u || u.length === 0) && i.suppressEmptyNode ? s += I + "/>" : u && u.endsWith(">") ? s += I + `>${u}${e}</${a}>` : (s += I + ">", u && e !== "" && (u.includes("/>") || u.includes("</")) ? s += e + i.indentBy + u + e : s += u, s += `</${a}>`), n = !0;
  }
  return s;
}
function Vl(c) {
  const i = Object.keys(c);
  for (let t = 0; t < i.length; t++) {
    const e = i[t];
    if (c.hasOwnProperty(e) && e !== ":@")
      return e;
  }
}
function Gr(c, i) {
  let t = "";
  if (c && !i.ignoreAttributes)
    for (let e in c) {
      if (!c.hasOwnProperty(e))
        continue;
      let s = i.attributeValueProcessor(e, c[e]);
      s = Co(s, i), s === !0 && i.suppressBooleanAttributes ? t += ` ${e.substr(i.attributeNamePrefix.length)}` : t += ` ${e.substr(i.attributeNamePrefix.length)}="${s}"`;
    }
  return t;
}
function Yl(c, i) {
  c = c.substr(0, c.length - i.textNodeName.length - 1);
  let t = c.substr(c.lastIndexOf(".") + 1);
  for (let e in i.stopNodes)
    if (i.stopNodes[e] === c || i.stopNodes[e] === "*." + t)
      return !0;
  return !1;
}
function Co(c, i) {
  if (c && c.length > 0 && i.processEntities)
    for (let t = 0; t < i.entities.length; t++) {
      const e = i.entities[t];
      c = c.replace(e.regex, e.val);
    }
  return c;
}
var Gl = Bl;
const zl = Gl, kl = {
  attributeNamePrefix: "@_",
  attributesGroupName: !1,
  textNodeName: "#text",
  ignoreAttributes: !0,
  cdataPropName: !1,
  format: !1,
  indentBy: "  ",
  suppressEmptyNode: !1,
  suppressUnpairedNode: !0,
  suppressBooleanAttributes: !0,
  tagValueProcessor: function(c, i) {
    return i;
  },
  attributeValueProcessor: function(c, i) {
    return i;
  },
  preserveOrder: !1,
  commentPropName: !1,
  unpairedTags: [],
  entities: [
    { regex: new RegExp("&", "g"), val: "&amp;" },
    //it must be on top
    { regex: new RegExp(">", "g"), val: "&gt;" },
    { regex: new RegExp("<", "g"), val: "&lt;" },
    { regex: new RegExp("'", "g"), val: "&apos;" },
    { regex: new RegExp('"', "g"), val: "&quot;" }
  ],
  processEntities: !0,
  stopNodes: [],
  // transformTagName: false,
  // transformAttributeName: false,
  oneListGroup: !1
};
function be(c) {
  this.options = Object.assign({}, kl, c), this.options.ignoreAttributes || this.options.attributesGroupName ? this.isAttribute = function() {
    return !1;
  } : (this.attrPrefixLen = this.options.attributeNamePrefix.length, this.isAttribute = Xl), this.processTextOrObjNode = Hl, this.options.format ? (this.indentate = Wl, this.tagEndChar = `>
`, this.newLine = `
`) : (this.indentate = function() {
    return "";
  }, this.tagEndChar = ">", this.newLine = "");
}
be.prototype.build = function(c) {
  return this.options.preserveOrder ? zl(c, this.options) : (Array.isArray(c) && this.options.arrayNodeName && this.options.arrayNodeName.length > 1 && (c = {
    [this.options.arrayNodeName]: c
  }), this.j2x(c, 0).val);
};
be.prototype.j2x = function(c, i) {
  let t = "", e = "";
  for (let s in c)
    if (Object.prototype.hasOwnProperty.call(c, s))
      if (typeof c[s] > "u")
        this.isAttribute(s) && (e += "");
      else if (c[s] === null)
        this.isAttribute(s) ? e += "" : s[0] === "?" ? e += this.indentate(i) + "<" + s + "?" + this.tagEndChar : e += this.indentate(i) + "<" + s + "/" + this.tagEndChar;
      else if (c[s] instanceof Date)
        e += this.buildTextValNode(c[s], s, "", i);
      else if (typeof c[s] != "object") {
        const n = this.isAttribute(s);
        if (n)
          t += this.buildAttrPairStr(n, "" + c[s]);
        else if (s === this.options.textNodeName) {
          let r = this.options.tagValueProcessor(s, "" + c[s]);
          e += this.replaceEntitiesValue(r);
        } else
          e += this.buildTextValNode(c[s], s, "", i);
      } else if (Array.isArray(c[s])) {
        const n = c[s].length;
        let r = "", o = "";
        for (let a = 0; a < n; a++) {
          const l = c[s][a];
          if (!(typeof l > "u"))
            if (l === null)
              s[0] === "?" ? e += this.indentate(i) + "<" + s + "?" + this.tagEndChar : e += this.indentate(i) + "<" + s + "/" + this.tagEndChar;
            else if (typeof l == "object")
              if (this.options.oneListGroup) {
                const h = this.j2x(l, i + 1);
                r += h.val, this.options.attributesGroupName && l.hasOwnProperty(this.options.attributesGroupName) && (o += h.attrStr);
              } else
                r += this.processTextOrObjNode(l, s, i);
            else if (this.options.oneListGroup) {
              let h = this.options.tagValueProcessor(s, l);
              h = this.replaceEntitiesValue(h), r += h;
            } else
              r += this.buildTextValNode(l, s, "", i);
        }
        this.options.oneListGroup && (r = this.buildObjectNode(r, s, o, i)), e += r;
      } else if (this.options.attributesGroupName && s === this.options.attributesGroupName) {
        const n = Object.keys(c[s]), r = n.length;
        for (let o = 0; o < r; o++)
          t += this.buildAttrPairStr(n[o], "" + c[s][n[o]]);
      } else
        e += this.processTextOrObjNode(c[s], s, i);
  return { attrStr: t, val: e };
};
be.prototype.buildAttrPairStr = function(c, i) {
  return i = this.options.attributeValueProcessor(c, "" + i), i = this.replaceEntitiesValue(i), this.options.suppressBooleanAttributes && i === "true" ? " " + c : " " + c + '="' + i + '"';
};
function Hl(c, i, t) {
  const e = this.j2x(c, t + 1);
  return c[this.options.textNodeName] !== void 0 && Object.keys(c).length === 1 ? this.buildTextValNode(c[this.options.textNodeName], i, e.attrStr, t) : this.buildObjectNode(e.val, i, e.attrStr, t);
}
be.prototype.buildObjectNode = function(c, i, t, e) {
  if (c === "")
    return i[0] === "?" ? this.indentate(e) + "<" + i + t + "?" + this.tagEndChar : this.indentate(e) + "<" + i + t + this.closeTag(i) + this.tagEndChar;
  {
    let s = "</" + i + this.tagEndChar, n = "";
    return i[0] === "?" && (n = "?", s = ""), (t || t === "") && c.indexOf("<") === -1 ? this.indentate(e) + "<" + i + t + n + ">" + c + s : this.options.commentPropName !== !1 && i === this.options.commentPropName && n.length === 0 ? this.indentate(e) + `<!--${c}-->` + this.newLine : this.indentate(e) + "<" + i + t + n + this.tagEndChar + c + this.indentate(e) + s;
  }
};
be.prototype.closeTag = function(c) {
  let i = "";
  return this.options.unpairedTags.indexOf(c) !== -1 ? this.options.suppressUnpairedNode || (i = "/") : this.options.suppressEmptyNode ? i = "/" : i = `></${c}`, i;
};
be.prototype.buildTextValNode = function(c, i, t, e) {
  if (this.options.cdataPropName !== !1 && i === this.options.cdataPropName)
    return this.indentate(e) + `<![CDATA[${c}]]>` + this.newLine;
  if (this.options.commentPropName !== !1 && i === this.options.commentPropName)
    return this.indentate(e) + `<!--${c}-->` + this.newLine;
  if (i[0] === "?")
    return this.indentate(e) + "<" + i + t + "?" + this.tagEndChar;
  {
    let s = this.options.tagValueProcessor(i, c);
    return s = this.replaceEntitiesValue(s), s === "" ? this.indentate(e) + "<" + i + t + this.closeTag(i) + this.tagEndChar : this.indentate(e) + "<" + i + t + ">" + s + "</" + i + this.tagEndChar;
  }
};
be.prototype.replaceEntitiesValue = function(c) {
  if (c && c.length > 0 && this.options.processEntities)
    for (let i = 0; i < this.options.entities.length; i++) {
      const t = this.options.entities[i];
      c = c.replace(t.regex, t.val);
    }
  return c;
};
function Wl(c) {
  return this.options.indentBy.repeat(c);
}
function Xl(c) {
  return c.startsWith(this.options.attributeNamePrefix) && c !== this.options.textNodeName ? c.substr(this.attrPrefixLen) : !1;
}
var $l = be;
const Zl = kn, jl = Ul, ql = $l;
var To = {
  XMLParser: jl,
  XMLValidator: Zl,
  XMLBuilder: ql
};
class wn {
  /**
   * Constructs a new BCF Topic Comment instance.
   * @param components - The Components instance.
   * @param text - The initial comment text.
   */
  constructor(i, t) {
    C(this, "date", /* @__PURE__ */ new Date());
    C(this, "author");
    C(this, "guid", oe.create());
    C(this, "viewpoint");
    C(this, "modifiedAuthor");
    C(this, "modifiedDate");
    C(this, "topic");
    C(this, "_components");
    C(this, "_comment", "");
    this._components = i, this._comment = t;
    const e = this._components.get(Mt);
    this.author = e.config.author;
  }
  /**
   * Sets the comment text and updates the modified date and author.
   * The author will be the one defined in BCFTopics.config.author
   * @param value - The new comment text.
   */
  set comment(i) {
    var e;
    const t = this._components.get(Mt);
    this._comment = i, this.modifiedDate = /* @__PURE__ */ new Date(), this.modifiedAuthor = t.config.author, (e = this.topic) == null || e.comments.set(this.guid, this);
  }
  /**
   * Gets the comment text.
   * @returns The comment text.
   */
  get comment() {
    return this._comment;
  }
  /**
   * Serializes the Comment instance into a BCF compliant XML string.
   *
   * @returns A string representing the Comment in BCFv2 XML format.
   */
  serialize() {
    let i = null;
    this.viewpoint && (i = `<Viewpoint Guid="${this.viewpoint.guid}"/>`);
    let t = null;
    this.modifiedDate && (t = `<ModifiedDate>${this.modifiedDate.toISOString()}</ModifiedDate>`);
    let e = null;
    return this.modifiedAuthor && (e = `<ModifiedAuthor>${this.modifiedAuthor}</ModifiedAuthor>`), `
      <Comment Guid="${this.guid}">
        <Date>${this.date.toISOString()}</Date>
        <Author>${this.author}</Author>
        <Comment>${this.comment}</Comment>
        ${i ?? ""}
        ${e ?? ""}
        ${t ?? ""}
      </Comment>
    `;
  }
}
const Ie = class Ie {
  /**
   * Initializes a new instance of the `Topic` class representing a BCF (BIM Collaboration Format) topic.
   * It provides methods and properties to manage and serialize BCF topics.
   *
   * @remarks
   * The default creationUser is the one set in BCFTopics.config.author
   * It should not be created manually. Better use BCFTopics.create().
   *
   * @param components - The `Components` instance that provides access to other components and services.
   */
  constructor(i) {
    /**
     * A unique identifier for the topic.
     *
     * @remarks
     * The `guid` is automatically generated upon topic creation and by no means it should change.
     */
    C(this, "guid", oe.create());
    C(this, "title", Ie.default.title);
    C(this, "creationDate", /* @__PURE__ */ new Date());
    C(this, "creationAuthor", "");
    // Store viewpoint guids instead of the actual Viewpoint to prevent a possible memory leak
    C(this, "viewpoints", new we());
    // Store topic guids instead of the actual Topic to prevent a possible memory leak
    C(this, "relatedTopics", new we());
    // There is no problem to store the comment it-self as it is not referenced anywhere else
    C(this, "comments", new re());
    C(this, "customData", {});
    C(this, "description");
    C(this, "serverAssignedId");
    C(this, "dueDate");
    C(this, "modifiedAuthor");
    C(this, "modifiedDate");
    C(this, "index");
    C(this, "_type", Ie.default.type);
    C(this, "_status", Ie.default.status);
    C(this, "_priority", Ie.default.priority);
    C(this, "_stage", Ie.default.stage);
    C(this, "_assignedTo", Ie.default.assignedTo);
    C(this, "_labels", Ie.default.labels);
    C(this, "_components");
    this._components = i;
    const t = i.get(Mt);
    this.creationAuthor = t.config.author, this.relatedTopics.guard = (e) => e !== this.guid;
  }
  set type(i) {
    const t = this._components.get(Mt), { strict: e, types: s } = t.config;
    (!e || s.has(i)) && (this._type = i);
  }
  get type() {
    return this._type;
  }
  set status(i) {
    const t = this._components.get(Mt), { strict: e, statuses: s } = t.config;
    (!e || s.has(i)) && (this._status = i);
  }
  get status() {
    return this._status;
  }
  set priority(i) {
    const t = this._components.get(Mt);
    if (i) {
      const { strict: e, priorities: s } = t.config;
      if (!(e ? s.has(i) : !0))
        return;
      this._priority = i;
    } else
      this._priority = i;
  }
  get priority() {
    return this._priority;
  }
  set stage(i) {
    const t = this._components.get(Mt);
    if (i) {
      const { strict: e, stages: s } = t.config;
      if (!(e ? s.has(i) : !0))
        return;
      this._stage = i;
    } else
      this._stage = i;
  }
  get stage() {
    return this._stage;
  }
  set assignedTo(i) {
    const t = this._components.get(Mt);
    if (i) {
      const { strict: e, users: s } = t.config;
      if (!(e ? s.has(i) : !0))
        return;
      this._assignedTo = i;
    } else
      this._assignedTo = i;
  }
  get assignedTo() {
    return this._assignedTo;
  }
  set labels(i) {
    const t = this._components.get(Mt), { strict: e, labels: s } = t.config;
    if (e) {
      const n = /* @__PURE__ */ new Set();
      for (const r of i)
        (!e || s.has(r)) && n.add(r);
      this._labels = n;
    } else
      this._labels = i;
  }
  get labels() {
    return this._labels;
  }
  get _managerVersion() {
    return this._components.get(Mt).config.version;
  }
  /**
   * Sets properties of the BCF Topic based on the provided data.
   *
   * @remarks
   * This method iterates over the provided `data` object and updates the corresponding properties of the BCF Topic.
   * It skips the `guid` property as it should not be modified.
   *
   * @param data - An object containing the properties to be updated.
   * @returns The topic
   *
   * @example
   * ```typescript
   * const topic = new Topic(components);
   * topic.set({
   *   title: "New BCF Topic Title",
   *   description: "This is a new description.",
   *   status: "Resolved",
   * });
   * ```
   */
  set(i) {
    const t = i, e = this;
    for (const n in i) {
      if (n === "guid")
        continue;
      const r = t[n];
      n in this && (e[n] = r);
    }
    return this._components.get(Mt).list.set(this.guid, this), this;
  }
  /**
   * Creates a new comment associated with the current topic.
   *
   * @param text - The text content of the comment.
   * @param viewpoint - (Optional) The viewpoint associated with the comment.
   *
   * @returns The newly created comment.
   *
   * @example
   * ```typescript
   * const viewpoint = viewpoints.create(world); // Created with an instance of Viewpoints
   * const topic = topics.create(); // Created with an instance of BCFTopics
   * topic.viewpoints.add(viewpoint);
   * const comment = topic.createComment("This is a new comment", viewpoint);
   * ```
   */
  createComment(i, t) {
    const e = new wn(this._components, i);
    return e.viewpoint = t, e.topic = this, this.comments.set(e.guid, e), e;
  }
  createLabelTags(i = this._managerVersion) {
    let t = "Labels";
    i === "2.1" && (t = "Labels"), i === "3" && (t = "Label");
    let e = [...this.labels].map((s) => `<${t}>${s}</${t}>`).join(`
`);
    for (const s in this.customData) {
      const n = this.customData[s];
      typeof n == "string" && (e += `
<${t}>${n}</${t}>`);
    }
    return i === "2.1" ? e : i === "3" ? e.length !== 0 ? `<Labels>
${e}
</Labels>` : "<Labels/>" : e;
  }
  createCommentTags(i = this._managerVersion) {
    const t = [...this.comments.values()].map((e) => e.serialize()).join(`
`);
    return i === "2.1" ? t : i === "3" ? t.length !== 0 ? `<Comments>
${t}
</Comments>` : "<Comments/>" : t;
  }
  createViewpointTags(i = this._managerVersion) {
    let t = "Viewpoints";
    i === "2.1" && (t = "Viewpoints"), i === "3" && (t = "ViewPoint");
    const e = this._components.get(ie), n = [...this.viewpoints].map((r) => e.list.get(r)).filter((r) => r).map((r) => `<${t} Guid="${r.guid}">
          <Viewpoint>${r.guid}.bcfv</Viewpoint>
          <Snapshot>${r.guid}.jpeg</Snapshot>
        </${t}>
      `).join(`
`);
    return i === "2.1" ? n : i === "3" ? n.length !== 0 ? `<Viewpoints>
${n}
</Viewpoints>` : "<Viewpoints />" : n;
  }
  createRelatedTopicTags(i = this._managerVersion) {
    const t = [...this.relatedTopics].map(
      (e) => `<RelatedTopic Guid="${e}"></RelatedTopic>
      `
    ).join(`
`);
    return i === "2.1" ? t : i === "3" ? t.length !== 0 ? `<RelatedTopics>
${t}
</RelatedTopics>` : "<RelatedTopics />" : t;
  }
  /**
   * Serializes the BCF Topic instance into an XML string representation based on the official schema.
   *
   * @remarks
   * This method constructs an XML string based on the properties of the BCF Topic instance.
   * It includes the topic's guid, type, status, creation date, creation author, priority, index,
   * modified date, modified author, due date, assigned to, description, stage, labels, related topics,
   * comments, and viewpoints.
   *
   * @returns A string representing the XML serialization of the BCF Topic.
   *
   * @example
   * ```typescript
   * const topic = bcfTopics.create(); // Created with an instance of BCFTopics
   * const xml = topic.serialize();
   * console.log(xml);
   * ```
   */
  serialize() {
    const i = this._managerVersion;
    let t = null;
    this.serverAssignedId && (t = `ServerAssignedId="${this.serverAssignedId}"`);
    let e = null;
    this.priority && (e = `<Priority>${this.priority}</Priority>`);
    let s = null;
    this.index && i === "2.1" && (s = `<Index>${this.index}</Index>`);
    let n = null;
    this.modifiedDate && (n = `<ModifiedDate>${this.modifiedDate.toISOString()}</ModifiedDate>`);
    let r = null;
    this.modifiedAuthor && (r = `<ModifiedAuthor>${this.modifiedAuthor}</ModifiedAuthor>`);
    let o = null;
    this.dueDate && (o = `<DueDate>${this.dueDate.toISOString()}</DueDate>`);
    let a = null;
    this.assignedTo && (a = `<AssignedTo>${this.assignedTo}</AssignedTo>`);
    let l = null;
    this.description && (l = `<Description>${this.description}</Description>`);
    let h = null;
    this.stage && (h = `<Stage>${this.stage}</Stage>`);
    const f = this.createCommentTags(i), I = this.createViewpointTags(i), u = this.createLabelTags(i), d = this.createRelatedTopicTags(i);
    return `
      <?xml version="1.0" encoding="UTF-8"?>
      <Markup>
        <Topic Guid="${this.guid}" TopicType="${this.type}" TopicStatus="${this.status}" ${t ?? ""}>
          <Title>${this.title}</Title>
          <CreationDate>${this.creationDate.toISOString()}</CreationDate>
          <CreationAuthor>${this.creationAuthor}</CreationAuthor>
          ${e ?? ""}
          ${s ?? ""}
          ${n ?? ""}
          ${r ?? ""}
          ${o ?? ""}
          ${a ?? ""}
          ${l ?? ""}
          ${h ?? ""}
          ${u}
          ${d}
          ${i === "3" ? f : ""}
          ${i === "3" ? I : ""}
        </Topic>
        ${i === "2.1" ? f : ""}
        ${i === "2.1" ? I : ""}
      </Markup>
    `;
  }
};
/**
 * Default values for a BCF Topic, excluding `guid`, `creationDate`, and `creationAuthor`.
 */
C(Ie, "default", {
  title: "BCF Topic",
  type: "Issue",
  status: "Active",
  labels: /* @__PURE__ */ new Set()
});
let Rs = Ie;
const Ql = (c, i) => {
  if (i.trim() === "")
    return;
  const t = Mt.xmlParser.parse(i).Extensions;
  if (!t)
    return;
  const { Priorities: e, TopicStatuses: s, TopicTypes: n, Users: r } = t;
  if (e && e.Priority) {
    const o = Array.isArray(e.Priority) ? e.Priority : [e.Priority];
    for (const a of o)
      c.config.priorities.add(a);
  }
  if (s && s.TopicStatus) {
    const o = Array.isArray(s.TopicStatus) ? s.TopicStatus : [s.TopicStatus];
    for (const a of o)
      c.config.statuses.add(a);
  }
  if (n && n.TopicType) {
    const o = Array.isArray(n.TopicType) ? n.TopicType : [n.TopicType];
    for (const a of o)
      c.config.types.add(a);
  }
  if (r && r.User) {
    const o = Array.isArray(r.User) ? r.User : [r.User];
    for (const a of o)
      c.config.users.add(a);
  }
};
class Kl extends Ge {
  constructor() {
    super(...arguments);
    C(this, "_config", {
      version: {
        type: "Select",
        options: /* @__PURE__ */ new Set(["2.1", "3"]),
        multiple: !1,
        value: ""
      },
      author: {
        type: "Text",
        value: ""
      },
      types: {
        type: "TextSet",
        value: /* @__PURE__ */ new Set()
      },
      statuses: {
        type: "TextSet",
        value: /* @__PURE__ */ new Set()
      },
      priorities: {
        type: "TextSet",
        value: /* @__PURE__ */ new Set()
      },
      labels: {
        type: "TextSet",
        value: /* @__PURE__ */ new Set()
      },
      stages: {
        type: "TextSet",
        value: /* @__PURE__ */ new Set()
      },
      users: {
        type: "TextSet",
        value: /* @__PURE__ */ new Set()
      },
      includeSelectionTag: {
        type: "Boolean",
        value: !1
      },
      updateExtensionsOnImport: {
        type: "Boolean",
        value: !1
      },
      strict: {
        type: "Boolean",
        value: !1
      },
      includeAllExtensionsOnExport: {
        type: "Boolean",
        value: !1
      },
      fallbackVersionOnImport: {
        type: "Select",
        multiple: !1,
        options: /* @__PURE__ */ new Set(["2.1", "3"]),
        value: ""
      },
      ignoreIncompleteTopicsOnImport: {
        type: "Boolean",
        value: !1
      }
    });
  }
  get version() {
    return this._config.version.value;
  }
  set version(t) {
    this._config.version.value = t;
  }
  get author() {
    return this._config.author.value;
  }
  set author(t) {
    this._config.author.value = t;
  }
  get types() {
    return this._config.types.value;
  }
  set types(t) {
    this._config.types.value = t;
  }
  get statuses() {
    return this._config.statuses.value;
  }
  set statuses(t) {
    this._config.statuses.value = t;
  }
  get priorities() {
    return this._config.priorities.value;
  }
  set priorities(t) {
    this._config.priorities.value = t;
  }
  get labels() {
    return this._config.labels.value;
  }
  set labels(t) {
    this._config.labels.value = t;
  }
  get stages() {
    return this._config.stages.value;
  }
  set stages(t) {
    this._config.stages.value = t;
  }
  get users() {
    return this._config.users.value;
  }
  set users(t) {
    this._config.users.value = t;
  }
  get includeSelectionTag() {
    return this._config.includeSelectionTag.value;
  }
  set includeSelectionTag(t) {
    this._config.includeSelectionTag.value = t;
  }
  get updateExtensionsOnImport() {
    return this._config.updateExtensionsOnImport.value;
  }
  set updateExtensionsOnImport(t) {
    this._config.updateExtensionsOnImport.value = t;
  }
  get strict() {
    return this._config.strict.value;
  }
  set strict(t) {
    this._config.strict.value = t;
  }
  get includeAllExtensionsOnExport() {
    return this._config.includeAllExtensionsOnExport.value;
  }
  set includeAllExtensionsOnExport(t) {
    this._config.includeAllExtensionsOnExport.value = t;
  }
  get fallbackVersionOnImport() {
    return this._config.fallbackVersionOnImport.value;
  }
  set fallbackVersionOnImport(t) {
    this._config.fallbackVersionOnImport.value = t;
  }
  get ignoreIncompleteTopicsOnImport() {
    return this._config.ignoreIncompleteTopicsOnImport.value;
  }
  set ignoreIncompleteTopicsOnImport(t) {
    this._config.ignoreIncompleteTopicsOnImport.value = t;
  }
}
const Le = class Le extends At {
  constructor() {
    super(...arguments);
    C(this, "enabled", !1);
    C(this, "_defaultConfig", {
      author: "jhon.doe@example.com",
      version: "2.1",
      types: /* @__PURE__ */ new Set([
        "Clash",
        "Failure",
        "Fault",
        "Inquiry",
        "Issue",
        "Remark",
        "Request"
      ]),
      statuses: /* @__PURE__ */ new Set(["Active", "In Progress", "Done", "In Review", "Closed"]),
      priorities: /* @__PURE__ */ new Set(["On hold", "Minor", "Normal", "Major", "Critical"]),
      labels: /* @__PURE__ */ new Set(),
      stages: /* @__PURE__ */ new Set(),
      users: /* @__PURE__ */ new Set(),
      includeSelectionTag: !1,
      updateExtensionsOnImport: !0,
      strict: !1,
      includeAllExtensionsOnExport: !0,
      fallbackVersionOnImport: "2.1",
      ignoreIncompleteTopicsOnImport: !1
    });
    C(this, "config", new Kl(
      this,
      this.components,
      "BCF Topics",
      Le.uuid
    ));
    C(this, "list", new re());
    C(this, "onSetup", new j());
    C(this, "isSetup", !1);
    C(this, "onBCFImported", new j());
    C(this, "onDisposed", new j());
  }
  setup(t) {
    if (this.isSetup)
      return;
    const e = { ...this._defaultConfig, ...t };
    this.config.version = e.version, this.config.author = e.author, this.config.types = e.types, this.config.statuses = e.statuses, this.config.priorities = e.priorities, this.config.labels = e.labels, this.config.stages = e.stages, this.config.users = e.users, this.config.includeSelectionTag = e.includeSelectionTag, this.config.updateExtensionsOnImport = e.updateExtensionsOnImport, this.config.strict = e.strict, this.config.includeAllExtensionsOnExport = e.includeAllExtensionsOnExport, this.config.fallbackVersionOnImport = e.fallbackVersionOnImport || "", this.config.ignoreIncompleteTopicsOnImport = e.ignoreIncompleteTopicsOnImport, this.isSetup = !0, this.enabled = !0, this.onSetup.trigger();
  }
  /**
   * Creates a new BCFTopic instance and adds it to the list.
   *
   * @param data - Optional partial BCFTopic object to initialize the new topic with.
   * If not provided, default values will be used.
   * @returns The newly created BCFTopic instance.
   */
  create(t) {
    const e = new Rs(this.components);
    return t && (e.guid = t.guid ?? e.guid, e.set(t)), this.list.set(e.guid, e), e;
  }
  /**
   * Disposes of the BCFTopics component and triggers the onDisposed event.
   *
   * @remarks
   * This method clears the list of topics and triggers the onDisposed event.
   * It also resets the onDisposed event listener.
   */
  dispose() {
    this.list.dispose(), this.onDisposed.trigger(), this.onDisposed.reset();
  }
  /**
   * Retrieves the unique set of topic types used across all topics.
   *
   * @returns A Set containing the unique topic types.
   */
  get usedTypes() {
    const t = [...this.list].map(([e, s]) => s.type);
    return new Set(t);
  }
  /**
   * Retrieves the unique set of topic statuses used across all topics.
   *
   * @returns A Set containing the unique topic statuses.
   */
  get usedStatuses() {
    const t = [...this.list].map(([e, s]) => s.status);
    return new Set(t);
  }
  /**
   * Retrieves the unique set of topic priorities used across all topics.
   *
   * @returns A Set containing the unique topic priorities.
   * Note: This method filters out any null or undefined priorities.
   */
  get usedPriorities() {
    const t = [...this.list].map(([e, s]) => s.priority).filter((e) => e);
    return new Set(t);
  }
  /**
   * Retrieves the unique set of topic stages used across all topics.
   *
   * @returns A Set containing the unique topic stages.
   * Note: This method filters out any null or undefined stages.
   */
  get usedStages() {
    const t = [...this.list].map(([e, s]) => s.stage).filter((e) => e);
    return new Set(t);
  }
  /**
   * Retrieves the unique set of users associated with topics.
   *
   * @returns A Set containing the unique users.
   * Note: This method collects users from the creation author, assigned to, modified author, and comment authors.
   */
  get usedUsers() {
    const t = [];
    for (const [e, s] of this.list) {
      t.push(s.creationAuthor), s.assignedTo && t.push(s.assignedTo), s.modifiedAuthor && t.push(s.modifiedAuthor);
      for (const [n, r] of s.comments)
        t.push(r.author), r.modifiedAuthor && t.push(r.modifiedAuthor);
    }
    return new Set(t);
  }
  /**
   * Retrieves the unique set of labels used across all topics.
   *
   * @returns A Set containing the unique labels.
   */
  get usedLabels() {
    const t = [];
    for (const [e, s] of this.list)
      t.push(...s.labels);
    return new Set(t);
  }
  /**
   * Updates the set of extensions (types, statuses, priorities, labels, stages, users) based on the current topics.
   * This method iterates through each topic in the list and adds its properties to the corresponding sets in the config.
   */
  updateExtensions() {
    for (const [t, e] of this.list) {
      for (const s of e.labels)
        this.config.labels.add(s);
      this.config.types.add(e.type), e.priority && this.config.priorities.add(e.priority), e.stage && this.config.stages.add(e.stage), this.config.statuses.add(e.status), this.config.users.add(e.creationAuthor), e.assignedTo && this.config.users.add(e.assignedTo), e.modifiedAuthor && this.config.users.add(e.modifiedAuthor);
      for (const [s, n] of e.comments)
        this.config.users.add(n.author), n.modifiedAuthor && this.config.users.add(n.modifiedAuthor);
    }
  }
  /**
   * Updates the references to viewpoints in the topics.
   * This function iterates through each topic and checks if the viewpoints exist in the viewpoints list.
   * If a viewpoint does not exist, it is removed from the topic's viewpoints.
   */
  updateViewpointReferences() {
    const t = this.components.get(ie);
    for (const [e, s] of this.list)
      for (const n of s.viewpoints)
        t.list.has(n) || s.viewpoints.delete(n);
  }
  /**
   * Exports the given topics to a BCF (Building Collaboration Format) zip file.
   *
   * @param topics - The topics to export. Defaults to all topics in the list.
   * @returns A promise that resolves to a Blob containing the exported BCF zip file.
   */
  async export(t = this.list.values()) {
    const e = new Ur();
    e.file(
      "bcf.version",
      `<?xml version="1.0" encoding="UTF-8"?>
    <Version VersionId="${this.config.version}" xsi:noNamespaceSchemaLocation="https://raw.githubusercontent.com/buildingSMART/BCF-XML/release_3_0/Schemas/version.xsd"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    </Version>`
    ), e.file("bcf.extensions", this.serializeExtensions());
    const n = await (await fetch(
      "https://thatopen.github.io/engine_components/resources/favicon.ico"
    )).arrayBuffer(), r = this.components.get(ie);
    for (const a of t) {
      const l = e.folder(a.guid);
      l.file("markup.bcf", a.serialize());
      for (const h of a.viewpoints) {
        const f = r.list.get(h);
        f && (l.file(`${h}.jpeg`, n, {
          binary: !0
        }), l.file(`${h}.bcfv`, await f.serialize()));
      }
    }
    return await e.generateAsync({ type: "blob" });
  }
  serializeExtensions() {
    const t = [...this.config.types].map((a) => `<TopicType>${a}</TopicType>`).join(`
`), e = [...this.config.statuses].map((a) => `<TopicStatus>${a}</TopicStatus>`).join(`
`), s = [...this.config.priorities].map((a) => `<Priority>${a}</Priority>`).join(`
`), n = [...this.config.labels].map((a) => `<TopicLabel>${a}</TopicLabel>`).join(`
`), r = [...this.config.stages].map((a) => `<Stage>${a}</Stage>`).join(`
`), o = [...this.config.users].map((a) => `<User>${a}</User>`).join(`
`);
    return `
      <?xml version="1.0" encoding="UTF-8"?>
      <Extensions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="your-schema-location.xsd">
        ${t.length !== 0 ? `<TopicTypes>
${t}
</TopicTypes>` : ""}
        ${e.length !== 0 ? `<TopicStatuses>
${e}
</TopicStatuses>` : ""}
        ${s.length !== 0 ? `<Priorities>
${s}
</Priorities>` : ""}
        ${n.length !== 0 ? `<TopicLabels>
${n}
</TopicLabels>` : ""}
        ${r.length !== 0 ? `<Stages>
${r}
</Stages>` : ""}
        ${o.length !== 0 ? `<Users>
${o}
</Users>` : ""}
      </Extensions>
    `;
  }
  processMarkupComment(t) {
    const {
      Guid: e,
      Date: s,
      Author: n,
      Comment: r,
      Viewpoint: o
    } = t;
    if (!(e && s && n && (wn || o)))
      return null;
    const a = this.components.get(ie), l = new wn(this.components, r ?? "");
    return l.guid = e, l.date = new Date(s), l.author = n, l.viewpoint = o != null && o.Guid ? a.list.get(o.Guid) : void 0, l.modifiedAuthor = t.ModifiedAuthor, l.modifiedDate = t.ModifiedDate ? new Date(t.ModifiedDate) : void 0, l;
  }
  getMarkupComments(t, e) {
    var o;
    let s;
    if (e === "2.1" && (s = t.Comment), e === "3" && (s = (o = t.Topic.Comments) == null ? void 0 : o.Comment), !s)
      return [];
    s = Array.isArray(s) ? s : [s];
    const n = s.map((a) => this.processMarkupComment(a)).filter((a) => a);
    return Array.isArray(n) ? n : [n];
  }
  getMarkupLabels(t, e) {
    var r;
    let s;
    return e === "2.1" && (s = t.Topic.Labels), e === "3" && (s = (r = t.Topic.Labels) == null ? void 0 : r.Label), s ? Array.isArray(s) ? s : [s] : [];
  }
  getMarkupViewpoints(t, e) {
    var n;
    let s;
    return e === "2.1" && (s = t.Viewpoints), e === "3" && (s = (n = t.Topic.Viewpoints) == null ? void 0 : n.ViewPoint), s ? (s = Array.isArray(s) ? s : [s], s) : [];
  }
  getMarkupRelatedTopics(t, e) {
    var r;
    let s;
    return e === "2.1" && (s = t.Topic.RelatedTopic), e === "3" && (s = (r = t.Topic.RelatedTopics) == null ? void 0 : r.RelatedTopic), s ? (Array.isArray(s) ? s : [s]).map((o) => o.Guid) : [];
  }
  /**
   * Loads BCF (Building Collaboration Format) data into the engine.
   *
   * @param world - The default world where the viewpoints are going to be created.
   * @param data - The BCF data to load.
   *
   * @returns A promise that resolves to an object containing the created viewpoints and topics.
   *
   * @throws An error if the BCF version is not supported.
   */
  async load(t, e) {
    var R;
    const {
      fallbackVersionOnImport: s,
      ignoreIncompleteTopicsOnImport: n,
      updateExtensionsOnImport: r
    } = this.config, o = new Ur();
    await o.loadAsync(t);
    const a = Object.values(o.files);
    let l = s;
    const h = a.find((S) => S.name.endsWith(".version"));
    if (h) {
      const S = await h.async("string"), m = Le.xmlParser.parse(S).Version.VersionId;
      l = String(m);
    }
    if (!(l && (l === "2.1" || l === "3")))
      throw new Error(`BCFTopics: ${l} is not supported.`);
    const f = a.find(
      (S) => S.name.endsWith(".extensions")
    );
    if (r && f) {
      const S = await f.async("string");
      Ql(this, S);
    }
    const I = [], u = this.components.get(ie), d = a.filter((S) => S.name.endsWith(".bcfv"));
    for (const S of d) {
      const m = await S.async("string"), F = Le.xmlParser.parse(m).VisualizationInfo;
      if (!F) {
        console.warn("Missing VisualizationInfo in Viewpoint");
        continue;
      }
      const O = {}, {
        Guid: y,
        ClippingPlanes: w,
        Components: L,
        OrthogonalCamera: b,
        PerspectiveCamera: Y
      } = F;
      if (y && (O.guid = y), L) {
        const { Selection: M, Visibility: g } = L;
        if (M && M.Component) {
          const q = Array.isArray(M.Component) ? M.Component : [M.Component];
          O.selectionComponents = q.map((G) => G.IfcGuid).filter((G) => G);
        }
        if (g && "DefaultVisibility" in g && (O.defaultVisibility = g.DefaultVisibility), g && g.Exceptions && "Component" in g.Exceptions) {
          const { Component: q } = g.Exceptions, G = Array.isArray(q) ? q : [q];
          O.exceptionComponents = G.map((et) => et.IfcGuid).filter((et) => et);
        }
        let v;
        l === "2.1" && (v = L.ViewSetupHints), l === "3" && (v = (R = L.Visibility) == null ? void 0 : R.ViewSetupHints), v && ("OpeningsVisible" in v && (O.openingsVisible = v.OpeningsVisible), "SpacesVisible" in v && (O.spacesVisible = v.SpacesVisible), "SpaceBoundariesVisible" in v && (O.spaceBoundariesVisible = v.SpaceBoundariesVisible));
      }
      if (b || Y) {
        const M = F.PerspectiveCamera ?? F.OrthogonalCamera, { CameraViewPoint: g, CameraDirection: v } = M, q = new D.Vector3(
          Number(g.X),
          Number(g.Z),
          Number(-g.Y)
        ), G = new D.Vector3(
          Number(v.X),
          Number(v.Z),
          Number(-v.Y)
        ), et = {
          position: { x: q.x, y: q.y, z: q.z },
          direction: { x: G.x, y: G.y, z: G.z },
          aspectRatio: "AspectRatio" in M ? M.AspectRatio : 1
          // Temporal simplification
        };
        "ViewToWorldScale" in M && (O.camera = {
          ...et,
          viewToWorldScale: M.ViewToWorldScale
        }), "FieldOfView" in M && (O.camera = {
          ...et,
          fov: M.FieldOfView
        });
      }
      const N = new mo(this.components, e, {
        data: O,
        setCamera: !1
      });
      if (L) {
        const { Coloring: M } = L;
        if (M && M.Color) {
          const g = Array.isArray(M.Color) ? M.Color : [M.Color];
          for (const v of g) {
            const { Color: q, Component: G } = v, W = (Array.isArray(G) ? G : [G]).map((nt) => nt.IfcGuid);
            N.componentColors.set(q, W);
          }
        }
      }
      if (I.push(N), w) {
        const M = this.components.get(On), g = Array.isArray(w.ClippingPlane) ? w.ClippingPlane : [w.ClippingPlane];
        for (const v of g) {
          const { Location: q, Direction: G } = v;
          if (!(q && G))
            continue;
          const et = new D.Vector3(
            q.X,
            q.Z,
            -q.Y
          ), W = new D.Vector3(
            G.X,
            -G.Z,
            G.Y
          ), nt = M.createFromNormalAndCoplanarPoint(
            e,
            W,
            et
          );
          nt.visible = !1, nt.enabled = !1, N.clippingPlanes.add(nt);
        }
      }
    }
    const E = {}, T = [], p = a.filter((S) => S.name.endsWith(".bcf"));
    for (const S of p) {
      const m = await S.async("string"), F = Le.xmlParser.parse(m).Markup, O = F.Topic, {
        Guid: y,
        TopicType: w,
        TopicStatus: L,
        Title: b,
        CreationDate: Y,
        CreationAuthor: N
      } = O;
      if (n && !(y && w && L && b && Y && N))
        continue;
      const M = new Rs(this.components);
      M.guid = y ?? M.guid;
      const g = this.getMarkupRelatedTopics(F, l);
      E[M.guid] = new Set(g), M.type = w ?? M.type, M.status = L ?? M.status, M.title = b ?? M.title, M.creationDate = Y ? new Date(Y) : M.creationDate, M.creationAuthor = N ?? M.creationAuthor, M.serverAssignedId = O.ServerAssignedId, M.priority = O.Priority, M.index = O.Index, M.modifiedDate = O.ModifiedDate ? new Date(O.ModifiedDate) : void 0, M.modifiedAuthor = O.ModifiedAuthor, M.dueDate = O.DueDate ? new Date(O.DueDate) : void 0, M.assignedTo = O.AssignedTo, M.description = O.Description, M.stage = O.Stage;
      const v = this.getMarkupLabels(F, l);
      for (const et of v)
        M.labels.add(et);
      const q = this.getMarkupComments(F, l);
      for (const et of q)
        M.comments.set(et.guid, et);
      const G = this.getMarkupViewpoints(F, l);
      for (const et of G) {
        if (!(et && et.Guid))
          continue;
        const W = u.list.get(et.Guid);
        W && M.viewpoints.add(W.guid);
      }
      this.list.set(M.guid, M), T.push(M);
    }
    for (const S in E) {
      const m = this.list.get(S);
      if (!m)
        continue;
      const F = E[S];
      for (const O of F)
        m.relatedTopics.add(O);
    }
    return this.onBCFImported.trigger(T), { viewpoints: I, topics: T };
  }
};
C(Le, "uuid", "de977976-e4f6-4e4f-a01a-204727839802"), C(Le, "xmlParser", new To.XMLParser({
  allowBooleanAttributes: !0,
  attributeNamePrefix: "",
  ignoreAttributes: !1,
  ignoreDeclaration: !0,
  ignorePiTags: !0,
  numberParseOptions: { leadingZeros: !0, hex: !0 },
  parseAttributeValue: !0,
  preserveOrder: !1,
  processEntities: !1,
  removeNSPrefix: !0,
  trimValues: !0
}));
let Mt = Le;
const ee = class ee extends At {
  constructor(t) {
    super(t);
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    C(this, "_absoluteMin");
    C(this, "_absoluteMax");
    C(this, "_meshes", []);
    this.components.add(ee.uuid, this), this._absoluteMin = ee.newBound(!0), this._absoluteMax = ee.newBound(!1);
  }
  /**
   * A static method to calculate the dimensions of a given bounding box.
   *
   * @param bbox - The bounding box to calculate the dimensions for.
   * @returns An object containing the width, height, depth, and center of the bounding box.
   */
  static getDimensions(t) {
    const { min: e, max: s } = t, n = Math.abs(s.x - e.x), r = Math.abs(s.y - e.y), o = Math.abs(s.z - e.z), a = new D.Vector3();
    return a.subVectors(s, e).divideScalar(2).add(e), { width: n, height: r, depth: o, center: a };
  }
  /**
   * A static method to create a new bounding box boundary.
   *
   * @param positive - A boolean indicating whether to create a boundary for positive or negative values.
   * @returns A new THREE.Vector3 representing the boundary.
   *
   * @remarks
   * This method is used to create a new boundary for calculating bounding boxes.
   * It sets the x, y, and z components of the returned vector to positive or negative infinity,
   * depending on the value of the `positive` parameter.
   *
   * @example
   * ```typescript
   * const positiveBound = BoundingBoxer.newBound(true);
   * console.log(positiveBound); // Output: Vector3 { x: Infinity, y: Infinity, z: Infinity }
   *
   * const negativeBound = BoundingBoxer.newBound(false);
   * console.log(negativeBound); // Output: Vector3 { x: -Infinity, y: -Infinity, z: -Infinity }
   * ```
   */
  static newBound(t) {
    const e = t ? 1 : -1;
    return new D.Vector3(
      e * Number.MAX_VALUE,
      e * Number.MAX_VALUE,
      e * Number.MAX_VALUE
    );
  }
  /**
   * A static method to calculate the bounding box of a set of points.
   *
   * @param points - An array of THREE.Vector3 representing the points.
   * @param min - An optional THREE.Vector3 representing the minimum bounds. If not provided, it will be calculated.
   * @param max - An optional THREE.Vector3 representing the maximum bounds. If not provided, it will be calculated.
   * @returns A THREE.Box3 representing the bounding box of the given points.
   *
   * @remarks
   * This method calculates the bounding box of a set of points by iterating through each point and updating the minimum and maximum bounds accordingly.
   * If the `min` or `max` parameters are provided, they will be used as the initial bounds. Otherwise, the initial bounds will be set to positive and negative infinity.
   *
   * @example
   * ```typescript
   * const points = [
   *   new THREE.Vector3(1, 2, 3),
   *   new THREE.Vector3(4, 5, 6),
   *   new THREE.Vector3(7, 8, 9),
   * ];
   *
   * const bbox = BoundingBoxer.getBounds(points);
   * console.log(bbox); // Output: Box3 { min: Vector3 { x: 1, y: 2, z: 3 }, max: Vector3 { x: 7, y: 8, z: 9 } }
   * ```
   */
  static getBounds(t, e, s) {
    const n = s || this.newBound(!1), r = e || this.newBound(!0);
    for (const o of t)
      o.x < r.x && (r.x = o.x), o.y < r.y && (r.y = o.y), o.z < r.z && (r.z = o.z), o.x > n.x && (n.x = o.x), o.y > n.y && (n.y = o.y), o.z > n.z && (n.z = o.z);
    return new D.Box3(e, s);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    const t = this.components.get(De);
    for (const e of this._meshes)
      t.destroy(e);
    this._meshes = [], this.onDisposed.trigger(ee.uuid), this.onDisposed.reset();
  }
  /**
   * Returns the bounding box of the calculated fragments.
   *
   * @returns A new THREE.Box3 instance representing the bounding box.
   *
   * @remarks
   * This method clones the internal minimum and maximum vectors and returns a new THREE.Box3 instance.
   * The returned box represents the bounding box of the calculated fragments.
   *
   * @example
   * ```typescript
   * const boundingBox = boundingBoxer.get();
   * console.log(boundingBox); // Output: Box3 { min: Vector3 { x: -10, y: -10, z: -10 }, max: Vector3 { x: 10, y: 10, z: 10 } }
   * ```
   */
  get() {
    const t = this._absoluteMin.clone(), e = this._absoluteMax.clone();
    return new D.Box3(t, e);
  }
  /**
   * Calculates and returns a sphere that encompasses the entire bounding box.
   *
   * @returns A new THREE.Sphere instance representing the calculated sphere.
   *
   * @remarks
   * This method calculates the center and radius of a sphere that encompasses the entire bounding box.
   * The center is calculated as the midpoint between the minimum and maximum bounds of the bounding box.
   * The radius is calculated as the distance from the center to the minimum bound.
   *
   * @example
   * ```typescript
   * const boundingBoxer = components.get(BoundingBoxer);
   * boundingBoxer.add(fragmentsGroup);
   * const boundingSphere = boundingBoxer.getSphere();
   * console.log(boundingSphere); // Output: Sphere { center: Vector3 { x: 0, y: 0, z: 0 }, radius: 10 }
   * ```
   */
  getSphere() {
    const t = this._absoluteMin.clone(), e = this._absoluteMax.clone(), s = Math.abs((e.x - t.x) / 2), n = Math.abs((e.y - t.y) / 2), r = Math.abs((e.z - t.z) / 2), o = new D.Vector3(t.x + s, t.y + n, t.z + r), a = o.distanceTo(t);
    return new D.Sphere(o, a);
  }
  /**
   * Returns a THREE.Mesh instance representing the bounding box.
   *
   * @returns A new THREE.Mesh instance representing the bounding box.
   *
   * @remarks
   * This method calculates the dimensions of the bounding box using the `getDimensions` method.
   * It then creates a new THREE.BoxGeometry with the calculated dimensions.
   * A new THREE.Mesh is created using the box geometry, and it is added to the `_meshes` array.
   * The position of the mesh is set to the center of the bounding box.
   *
   * @example
   * ```typescript
   * const boundingBoxer = components.get(BoundingBoxer);
   * boundingBoxer.add(fragmentsGroup);
   * const boundingBoxMesh = boundingBoxer.getMesh();
   * scene.add(boundingBoxMesh);
   * ```
   */
  getMesh() {
    const t = new D.Box3(this._absoluteMin, this._absoluteMax), e = ee.getDimensions(t), { width: s, height: n, depth: r, center: o } = e, a = new D.BoxGeometry(s, n, r), l = new D.Mesh(a);
    return this._meshes.push(l), l.position.copy(o), l;
  }
  /**
   * Resets the internal minimum and maximum vectors to positive and negative infinity, respectively.
   * This method is used to prepare the BoundingBoxer for a new set of fragments.
   *
   * @remarks
   * This method is called when a new set of fragments is added to the BoundingBoxer.
   * It ensures that the bounding box calculations are accurate and up-to-date.
   *
   * @example
   * ```typescript
   * const boundingBoxer = components.get(BoundingBoxer);
   * boundingBoxer.add(fragmentsGroup);
   * // ...
   * boundingBoxer.reset();
   * ```
   */
  reset() {
    this._absoluteMin = ee.newBound(!0), this._absoluteMax = ee.newBound(!1);
  }
  /**
   * Adds a FragmentsGroup to the BoundingBoxer.
   *
   * @param group - The FragmentsGroup to add.
   *
   * @remarks
   * This method iterates through each fragment in the provided FragmentsGroup,
   * and calls the `addMesh` method for each fragment's mesh.
   *
   * @example
   * ```typescript
   * const boundingBoxer = components.get(BoundingBoxer);
   * boundingBoxer.add(fragmentsGroup);
   * ```
   */
  add(t) {
    for (const e of t.items)
      this.addMesh(e.mesh);
  }
  /**
   * Adds a mesh to the BoundingBoxer and calculates the bounding box.
   *
   * @param mesh - The mesh to add. It can be an instance of THREE.InstancedMesh, THREE.Mesh, or FRAGS.CurveMesh.
   * @param itemIDs - An optional iterable of numbers representing the item IDs.
   *
   * @remarks
   * This method calculates the bounding box of the provided mesh and updates the internal minimum and maximum vectors.
   * If the mesh is an instance of THREE.InstancedMesh, it calculates the bounding box for each instance.
   * If the mesh is an instance of FRAGS.FragmentMesh and itemIDs are provided, it calculates the bounding box for the specified item IDs.
   *
   * @example
   * ```typescript
   * const boundingBoxer = components.get(BoundingBoxer);
   * boundingBoxer.addMesh(mesh);
   * ```
   */
  addMesh(t, e) {
    if (!t.geometry.index)
      return;
    const s = ee.getFragmentBounds(t);
    t.updateMatrixWorld();
    const n = t.matrixWorld, r = new D.Matrix4(), o = t instanceof D.InstancedMesh, a = /* @__PURE__ */ new Set();
    if (t instanceof Qt.FragmentMesh) {
      e || (e = t.fragment.ids);
      for (const l of e) {
        const h = t.fragment.getInstancesIDs(l);
        if (h)
          for (const f of h)
            a.add(f);
      }
    } else
      a.add(0);
    for (const l of a) {
      const h = s.min.clone(), f = s.max.clone();
      o && (t.getMatrixAt(l, r), h.applyMatrix4(r), f.applyMatrix4(r)), h.applyMatrix4(n), f.applyMatrix4(n), h.x < this._absoluteMin.x && (this._absoluteMin.x = h.x), h.y < this._absoluteMin.y && (this._absoluteMin.y = h.y), h.z < this._absoluteMin.z && (this._absoluteMin.z = h.z), h.x > this._absoluteMax.x && (this._absoluteMax.x = h.x), h.y > this._absoluteMax.y && (this._absoluteMax.y = h.y), h.z > this._absoluteMax.z && (this._absoluteMax.z = h.z), f.x > this._absoluteMax.x && (this._absoluteMax.x = f.x), f.y > this._absoluteMax.y && (this._absoluteMax.y = f.y), f.z > this._absoluteMax.z && (this._absoluteMax.z = f.z), f.x < this._absoluteMin.x && (this._absoluteMin.x = f.x), f.y < this._absoluteMin.y && (this._absoluteMin.y = f.y), f.z < this._absoluteMin.z && (this._absoluteMin.z = f.z);
    }
  }
  /**
   * Uses a FragmentIdMap to add its meshes to the bb calculation.
   *
   * This method iterates through the provided `fragmentIdMap`, retrieves the corresponding fragment from the `FragmentsManager`,
   * and then calls the `addMesh` method for each fragment's mesh, passing the expression IDs as the second parameter.
   *
   * @param fragmentIdMap - A mapping of fragment IDs to their corresponding expression IDs.
   *
   * @remarks
   * This method is used to add a mapping of fragment IDs to their corresponding expression IDs.
   * It ensures that the bounding box calculations are accurate and up-to-date by updating the internal minimum and maximum vectors.
   *
   * @example
   * ```typescript
   * const boundingBoxer = components.get(BoundingBoxer);
   * const fragmentIdMap: FRAGS.FragmentIdMap = {
   *   '5991fa75-2eef-4825-90b3-85177f51a9c9': [123, 245, 389],
   *   '3469077e-39bf-4fc9-b3e6-4a1d78ad52b0': [454, 587, 612],
   * };
   * boundingBoxer.addFragmentIdMap(fragmentIdMap);
   * ```
   */
  addFragmentIdMap(t) {
    const e = this.components.get(Tt);
    for (const s in t) {
      const n = e.list.get(s);
      if (!n)
        continue;
      const r = t[s];
      this.addMesh(n.mesh, r);
    }
  }
  static getFragmentBounds(t) {
    const e = t.geometry.attributes.position, s = Number.MAX_VALUE, n = -s, r = new D.Vector3(s, s, s), o = new D.Vector3(n, n, n);
    if (!t.geometry.index)
      throw new Error("Geometry must be indexed!");
    const a = Array.from(t.geometry.index.array);
    for (let l = 0; l < a.length; l++) {
      if (l % 3 === 0 && a[l] === 0 && a[l + 1] === 0 && a[l + 2] === 0) {
        l += 2;
        continue;
      }
      const h = a[l], f = e.getX(h), I = e.getY(h), u = e.getZ(h);
      f < r.x && (r.x = f), I < r.y && (r.y = I), u < r.z && (r.z = u), f > o.x && (o.x = f), I > o.y && (o.y = I), u > o.z && (o.z = u);
    }
    return new D.Box3(r, o);
  }
};
C(ee, "uuid", "d1444724-dba6-4cdd-a0c7-68ee1450d166");
let Mn = ee;
const _s = class _s {
  constructor(i) {
    /**
     * Event used to notify the progress when performing a query on an IFC file.
     */
    C(this, "onProgress", new j());
    /**
     * If false, ALL rules of the query must comply to make a match. If true, ANY rule will be enough to make a match.
     */
    C(this, "inclusive", !1);
    /**
     * The list of rules to be applied by this query.
     */
    C(this, "rules", []);
    /**
     * The IDs of the match items per model.
     */
    C(this, "ids", {});
    /**
     * Whether this query is up to date or not per file. If not, when updating the group where it belongs, it will re-process the given file.
     */
    C(this, "needsUpdate", /* @__PURE__ */ new Map());
    C(this, "components");
    this.components = i;
  }
  /**
   * Imports a query given its data. This data can be generating using its {@link IfcFinderQuery.export} method.
   *
   * @param components the instance of {@link Components} used by this app.
   * @param data the data of the query to import as a serializable object.
   */
  static import(i, t) {
    const e = _s.importers.get(t.type);
    return e ? e(i, t) : (console.warn("Invalid query data:.", t), null);
  }
  /**
   * Imports the given serialized rules. Only use this when writing your own custom query. See the other queries provided by the library for reference.
   *
   * @param serializedRules the rules to be parsed.
   */
  static importRules(i) {
    const t = [];
    for (const e of i) {
      const s = {};
      for (const n in e) {
        const r = e[n];
        r.regexp ? s[n] = new RegExp(r.value) : s[n] = r;
      }
      t.push(s);
    }
    return t;
  }
  /**
   * Imports the given IDs. Only use this when writing your own custom query. See the other queries provided by the library for reference.
   *
   * @param data the serialized object representing the query whose IDs to parse.
   */
  static importIds(i) {
    const t = {};
    for (const e in i.ids)
      t[e] = new Set(i.ids[e]);
    return t;
  }
  /**
   * Clears the data of the given model. If not specified, clears all the data.
   *
   * @param modelID ID of the model whose data to clear.
   */
  clear(i) {
    if (i === void 0) {
      this.ids = {}, this.needsUpdate.clear();
      return;
    }
    delete this.ids[i], this.needsUpdate.delete(i);
  }
  addID(i, t) {
    this.ids[i] || (this.ids[i] = /* @__PURE__ */ new Set()), this.ids[i].add(t);
  }
  getData() {
    const i = {};
    for (const e in this.ids)
      i[e] = Array.from(this.ids[e]);
    const t = this.exportRules();
    return {
      name: this.name,
      inclusive: this.inclusive,
      type: "IfcFinderQuery",
      ids: i,
      rules: t
    };
  }
  exportRules() {
    const i = [];
    for (const t of this.rules) {
      const e = {};
      for (const s in t) {
        const n = t[s];
        n instanceof RegExp ? e[s] = { regexp: !0, value: n.source } : e[s] = n;
      }
      i.push(e);
    }
    return i;
  }
  findInFile(i, t) {
    return new Promise((e) => {
      const s = new FileReader(), n = new TextDecoder("utf-8"), r = 1e4 * 1024, o = 1e3;
      let a = 0;
      const l = /;/, h = () => {
        if (a >= t.size) {
          e();
          return;
        }
        const f = Math.min(a + r + o, t.size), I = t.slice(a, f);
        s.readAsArrayBuffer(I);
      };
      s.onload = () => {
        if (!(s.result instanceof ArrayBuffer))
          return;
        const f = new Uint8Array(s.result), u = n.decode(f).split(l);
        u.shift(), this.findInLines(i, u), this.onProgress.trigger(a / t.size), a += r, h();
      }, h();
    });
  }
  getIdFromLine(i) {
    const t = i.slice(i.indexOf("#") + 1, i.indexOf("="));
    return parseInt(t, 10);
  }
  testRules(i) {
    let t = null, e = null, s = null, n = !1;
    for (const r of this.rules) {
      if (r.type === "category") {
        if (t === null && (t = this.getCategoryFromLine(i), t === null)) {
          if (this.inclusive)
            continue;
          break;
        }
        if (!r.value.test(t)) {
          if (this.inclusive)
            continue;
          n = !1;
          break;
        }
        n = !0;
        continue;
      }
      if (e === null && (e = this.getAttributesFromLine(i), e === null)) {
        if (this.inclusive)
          continue;
        n = !1;
        break;
      }
      if (t === null && (t = this.getCategoryFromLine(i), t === null)) {
        if (this.inclusive)
          continue;
        n = !1;
        break;
      }
      if (s === null && (s = Object.keys(new k.IFC4[t]()), s = s.slice(2), s === null)) {
        if (this.inclusive)
          continue;
        n = !1;
        break;
      }
      if (r.type === "property") {
        const { name: o, value: a } = r;
        if (!a.test(i)) {
          if (this.inclusive)
            continue;
          n = !1;
          break;
        }
        let l = !1;
        for (let h = 0; h < e.length; h++) {
          const f = e[h], I = s[h];
          if (a.test(f) && o.test(I)) {
            l = !0;
            break;
          }
        }
        if (l)
          n = !0;
        else if (!this.inclusive) {
          n = !1;
          break;
        }
      }
      if (r.type === "operator") {
        const { name: o, value: a, operator: l } = r;
        let h = !1;
        for (let f = 0; f < e.length; f++) {
          const I = s[f], u = e[f].replace(
            /IFCLENGTHMEASURE\(|IFCVOLUMEMEASURE\(|\)/g,
            ""
          );
          if (o.test(I)) {
            if (l === "=" && parseFloat(u) === a) {
              h = !0;
              break;
            } else if (l === "<" && parseFloat(u) < a) {
              h = !0;
              break;
            } else if (l === ">" && parseFloat(u) > a) {
              h = !0;
              break;
            } else if (l === ">=" && parseFloat(u) >= a) {
              h = !0;
              break;
            } else if (l === "<=" && parseFloat(u) <= a) {
              h = !0;
              break;
            }
          }
        }
        if (h)
          n = !0;
        else if (!this.inclusive) {
          n = !1;
          break;
        }
      }
    }
    return n;
  }
  getCategoryFromLine(i) {
    const t = i.indexOf("=") + 1, e = i.indexOf("("), s = i.slice(t, e).trim(), n = wc[s];
    return n || null;
  }
  getAttributesFromLine(i) {
    const t = /\((.*)\)/, e = i.match(t);
    if (!(e && e[1]))
      return null;
    const s = /,(?![^()]*\))/g;
    return e[1].split(s).map((r) => r.trim());
  }
};
/**
 * The list of functions to import the queries. If you create your own custom query, you should add its importer here. See the other queries provided by the library for reference.
 */
C(_s, "importers", /* @__PURE__ */ new Map());
let se = _s;
class zr {
  constructor(i) {
    /**
     * The list of queries contained in this group.
     */
    C(this, "list", /* @__PURE__ */ new Map());
    /**
     * A unique string to identify this group instance.
     */
    C(this, "id", D.MathUtils.generateUUID());
    /**
     * The way this group works when retrieving items.
     * - Combine: returns the sum of all items of all queries.
     * - Intersect: returns only the common elements of all queries.
     */
    C(this, "mode", "intersect");
    C(this, "_components");
    this._components = i;
  }
  /**
   * The list of unique queries contained in this group.
   */
  get queries() {
    return new Set(this.list.values());
  }
  /**
   * The items of all the queries contained in this group. The returned data depends on {@link IfcQueryGroup.mode}.
   */
  get items() {
    const i = [];
    for (const t of this.queries)
      i.push(t.items);
    return this.mode === "combine" ? Qt.FragmentUtils.combine(i) : Qt.FragmentUtils.intersect(i);
  }
  /**
   * Adds a new query to this group.
   * @param query the query to add.
   */
  add(i) {
    if (this.list.has(i.name))
      throw new Error(
        `This group already has a query with the name ${i.name}.`
      );
    this.list.set(i.name, i);
  }
  /**
   * Clears the data of the given modelID of all queries contained in this group. If no modelID is provided, clears all data.
   * @param modelID the model whose data to remove.
   */
  clear(i) {
    for (const t of this.queries)
      t.clear(i);
  }
  /**
   * Imports data that has been previously exported through {@link IfcQueryGroup.export}.
   * @param data the serializable object used to persist a group's data.
   */
  import(i) {
    this.mode = i.mode, this.id = i.id;
    for (const t in i.queries) {
      const e = se.import(this._components, i.queries[t]);
      e && this.list.set(t, e);
    }
  }
  /**
   * Exports all the data of this group, so that it can be persisted and imported later using {@link IfcQueryGroup.import}.
   */
  export() {
    const i = {};
    for (const [t, e] of this.list)
      i[t] = e.export();
    return {
      mode: this.mode,
      id: this.id,
      queries: i
    };
  }
  /**
   * Updates all the queries contained in this group that need an update for the given file. It will skip those where {@link IfcFinderQuery.needsUpdate} is false.
   * @param modelID the identifier used to refer to the given file.
   * @param file the file to process.
   */
  async update(i, t) {
    for (const e of this.queries) {
      const s = e.needsUpdate.get(i);
      (s === void 0 || s) && await e.update(i, t);
    }
  }
}
const ws = class ws extends se {
  constructor(t, e) {
    super(t);
    /**
     * {@link IfcFinderQuery.name}
     */
    C(this, "name");
    this.name = e.name, this.rules = e.rules, this.inclusive = e.inclusive;
  }
  /**
   * {@link IfcFinderQuery.items}
   */
  get items() {
    const t = this.components.get(Tt), e = [];
    for (const s in this.ids) {
      const n = this.ids[s], r = t.groups.get(s);
      if (!r) {
        console.warn(`Model ${s} not found!`);
        continue;
      }
      const o = r.getFragmentMap(n);
      e.push(o);
    }
    return Qt.FragmentUtils.combine(e);
  }
  /**
   * {@link IfcFinderQuery.export}
   */
  export() {
    const t = this.getData();
    return t.type = ws.type, t;
  }
  /**
   * {@link IfcFinderQuery.update}
   */
  async update(t, e) {
    this.ids[t] = /* @__PURE__ */ new Set(), await this.findInFile(t, e), this.needsUpdate.set(t, !1);
  }
  findInLines(t, e) {
    for (const s of e)
      if (this.testRules(s)) {
        const r = this.getIdFromLine(s);
        this.addID(t, r);
      }
  }
};
/**
 * The type of this query.
 */
C(ws, "type", "IfcBasicQuery");
let gs = ws;
se.importers.set(
  gs.type,
  (c, i) => {
    const t = new gs(c, {
      name: i.name,
      rules: se.importRules(i.rules),
      inclusive: i.inclusive
    });
    return t.ids = se.importIds(i), t;
  }
);
const Ms = class Ms extends se {
  constructor(t, e) {
    super(t);
    /**
     * {@link IfcFinderQuery.name}
     */
    C(this, "name");
    C(this, "psets", []);
    this.name = e.name, this.rules = e.rules, this.inclusive = e.inclusive;
  }
  /**
   * {@link IfcFinderQuery.items}
   */
  get items() {
    const t = this.components.get(Gt), e = this.components.get(Tt), s = [];
    for (const n in this.ids) {
      const r = e.groups.get(n);
      if (!r) {
        console.log(`Model not found: ${n}.`);
        continue;
      }
      const o = this.ids[n];
      for (const a of o) {
        const l = t.getEntityRelations(
          n,
          a,
          "DefinesOcurrence"
        );
        if (l) {
          const h = r.getFragmentMap(l);
          s.push(h);
        }
      }
    }
    return Qt.FragmentUtils.combine(s);
  }
  /**
   * {@link IfcFinderQuery.export}
   */
  export() {
    const t = this.getData();
    return t.type = Ms.type, t;
  }
  /**
   * {@link IfcFinderQuery.update}
   */
  async update(t, e) {
    await this.findInFile(t, e);
    const s = /* @__PURE__ */ new Set();
    for (const n of this.psets) {
      const r = this.getAttributesFromLine(n);
      if (r === null)
        continue;
      const o = r[4].replace("(", "[").replace(")", "]").replace(/#/g, ""), a = JSON.parse(o);
      for (const l of a) {
        const h = this.ids[t];
        if (h && h.has(l)) {
          const f = this.getIdFromLine(n);
          s.add(f);
          break;
        }
      }
    }
    this.ids[t] = s, this.psets = [], this.needsUpdate.set(t, !1);
  }
  findInLines(t, e) {
    for (const s of e) {
      const n = this.getCategoryFromLine(s);
      if (n === "IfcPropertySet") {
        this.psets.push(s);
        continue;
      }
      if (n !== "IfcPropertySingleValue")
        continue;
      if (this.testRules(s)) {
        const o = this.getIdFromLine(s);
        this.addID(t, o);
      }
    }
  }
};
/**
 * The type of this query.
 */
C(Ms, "type", "IfcPropertyQuery");
let As = Ms;
se.importers.set(
  As.type,
  (c, i) => {
    const t = new As(c, {
      name: i.name,
      inclusive: i.inclusive,
      rules: se.importRules(i.rules)
    });
    return t.ids = se.importIds(i), t;
  }
);
const Ds = class Ds extends At {
  constructor(t) {
    super(t);
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    /**
     * List of all created {@link IfcQueryGroup} instances.
     */
    C(this, "list", /* @__PURE__ */ new Map());
    t.add(Ds.uuid, this);
  }
  /**
   * List of all queries from all created {@link IfcQueryGroup} instances.
   */
  get queries() {
    const t = /* @__PURE__ */ new Set();
    for (const [, e] of this.list)
      for (const s of e.queries)
        t.add(s);
    return t;
  }
  /**
   * Imports all the query groups provided in the given data. You can generate this data to save the result of queries and persist it over time.
   * @param data The data containing the serialized query groups to import.
   */
  import(t) {
    for (const e in t) {
      const s = new zr(this.components);
      s.import(t[e]), this.list.set(e, s);
    }
  }
  /**
   * Exports all the query groups created. You can then import this data back using the import method.
   */
  export() {
    const t = {};
    for (const [e, s] of this.list)
      t[e] = s.export();
    return t;
  }
  /**
   * Creates a new {@link IfcQueryGroup}.
   */
  create() {
    const t = new zr(this.components);
    return this.list.set(t.id, t), t;
  }
  /**
   * Creates the {@link IfcQueryGroup} with the given ID.
   */
  delete(t) {
    this.list.delete(t);
  }
  /**
   * Deletes all {@link IfcQueryGroup} instances.
   */
  clear() {
    this.list.clear();
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(Ds, "uuid", "0da7ad77-f734-42ca-942f-a074adfd1e3a");
let kr = Ds;
const bs = class bs extends At {
  constructor(t) {
    super(t);
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    /**
     * A map representing the classification systems.
     * The key is the system name, and the value is an object representing the classes within the system.
     */
    C(this, "list", {});
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    C(this, "onFragmentsDisposed", (t) => {
      const { groupID: e, fragmentIDs: s } = t;
      for (const n in this.list) {
        const r = this.list[n], o = Object.keys(r);
        if (o.includes(e))
          delete r[e], Object.values(r).length === 0 && delete this.list[n];
        else
          for (const a of o) {
            const l = r[a];
            for (const h of s)
              delete l.map[h];
            Object.values(l).length === 0 && delete r[a];
          }
      }
    });
    t.add(bs.uuid, this), t.get(Tt).onFragmentsDisposed.add(this.onFragmentsDisposed);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.list = {}, this.components.get(Tt).onFragmentsDisposed.remove(this.onFragmentsDisposed), this.onDisposed.trigger(), this.onDisposed.reset();
  }
  /**
   * Removes a fragment from the classification based on its unique identifier (guid).
   * This method iterates through all classification systems and classes, and deletes the fragment with the specified guid from the respective group.
   *
   * @param guid - The unique identifier of the fragment to be removed.
   */
  remove(t) {
    for (const e in this.list) {
      const s = this.list[e];
      for (const n in s) {
        const r = s[n];
        delete r.map[t];
      }
    }
  }
  /**
   * Finds and returns fragments based on the provided filter criteria.
   * If no filter is provided, it returns all fragments.
   *
   * @param filter - An optional object containing filter criteria.
   * The keys of the object represent the classification system names,
   * and the values are arrays of class names to match.
   *
   * @returns A map of fragment GUIDs to their respective express IDs,
   * where the express IDs are filtered based on the provided filter criteria.
   *
   * @throws Will throw an error if the fragments map is malformed.
   */
  find(t) {
    const e = this.components.get(Tt);
    if (!t) {
      const o = {};
      for (const [a, l] of e.list)
        o[a] = new Set(l.ids);
      return o;
    }
    const s = Object.keys(t).length, n = {};
    for (const o in t) {
      const a = t[o];
      if (!this.list[o]) {
        console.warn(`Classification ${o} does not exist.`);
        continue;
      }
      for (const l of a) {
        const h = this.list[o][l];
        if (h)
          for (const f in h.map) {
            n[f] || (n[f] = /* @__PURE__ */ new Map());
            for (const I of h.map[f]) {
              const u = n[f].get(I);
              u === void 0 ? n[f].set(I, 1) : n[f].set(I, u + 1);
            }
          }
      }
    }
    const r = {};
    for (const o in n) {
      const a = n[o];
      for (const [l, h] of a) {
        if (h === void 0)
          throw new Error("Malformed fragments map!");
        h === s && (r[o] || (r[o] = /* @__PURE__ */ new Set()), r[o].add(l));
      }
    }
    return r;
  }
  /**
   * Classifies fragments based on their modelID.
   *
   * @param modelID - The unique identifier of the model to classify fragments by.
   * @param group - The FragmentsGroup containing the fragments to be classified.
   *
   * @remarks
   * This method iterates through the fragments in the provided group,
   * and classifies them based on their modelID.
   * The classification is stored in the `list.models` property,
   * with the modelID as the key and a map of fragment IDs to their respective express IDs as the value.
   *
   */
  byModel(t, e) {
    this.list.models || (this.list.models = {});
    const s = this.list.models;
    s[t] || (s[t] = { map: {}, id: null, name: t });
    const n = s[t];
    for (const [r, o] of e.data) {
      const a = o[0];
      for (const l of a) {
        const h = e.keyFragments.get(l);
        h && (n.map[h] || (n.map[h] = /* @__PURE__ */ new Set()), n.map[h].add(r));
      }
    }
  }
  /**
   * Classifies fragments based on their PredefinedType property.
   *
   * @param group - The FragmentsGroup containing the fragments to be classified.
   *
   * @remarks
   * This method iterates through the properties of the fragments in the provided group,
   * and classifies them based on their PredefinedType property.
   * The classification is stored in the `list.predefinedTypes` property,
   * with the PredefinedType as the key and a map of fragment IDs to their respective express IDs as the value.
   *
   * @throws Will throw an error if the fragment ID is not found.
   */
  async byPredefinedType(t) {
    var n;
    this.list.predefinedTypes || (this.list.predefinedTypes = {});
    const e = this.list.predefinedTypes, s = t.getAllPropertiesIDs();
    for (const r of s) {
      const o = await t.getProperties(r);
      if (!o)
        continue;
      const a = String((n = o.PredefinedType) == null ? void 0 : n.value).toUpperCase();
      e[a] || (e[a] = {
        map: {},
        id: null,
        name: a
      });
      const l = e[a];
      for (const [h, f] of t.data) {
        const I = f[0];
        for (const u of I) {
          const d = t.keyFragments.get(u);
          if (!d)
            throw new Error("Fragment ID not found!");
          l.map[d] || (l.map[d] = /* @__PURE__ */ new Set()), l.map[d].add(o.expressID);
        }
      }
    }
  }
  /**
   * Classifies fragments based on their entity type.
   *
   * @param group - The FragmentsGroup containing the fragments to be classified.
   *
   * @remarks
   * This method iterates through the relations of the fragments in the provided group,
   * and classifies them based on their entity type.
   * The classification is stored in the `list.entities` property,
   * with the entity type as the key and a map of fragment IDs to their respective express IDs as the value.
   *
   * @throws Will throw an error if the fragment ID is not found.
   */
  byEntity(t) {
    this.list.entities || (this.list.entities = {});
    for (const [e, s] of t.data) {
      const r = s[1][1], o = ms[r];
      this.saveItem(t, "entities", o, e);
    }
  }
  /**
   * Classifies fragments based on a specific IFC relationship.
   *
   * @param group - The FragmentsGroup containing the fragments to be classified.
   * @param ifcRel - The IFC relationship number to classify fragments by.
   * @param systemName - The name of the classification system to store the classification.
   *
   * @remarks
   * This method iterates through the relations of the fragments in the provided group,
   * and classifies them based on the specified IFC relationship.
   * The classification is stored in the `list` property under the specified system name,
   * with the relationship name as the class name and a map of fragment IDs to their respective express IDs as the value.
   *
   * @throws Will throw an error if the fragment ID is not found or if the IFC relationship is not valid.
   */
  async byIfcRel(t, e, s) {
    oi.isRel(e) && await oi.getRelationMap(
      t,
      e,
      async (n, r) => {
        const { name: o } = await oi.getEntityName(
          t,
          n
        );
        for (const a of r)
          this.saveItem(
            t,
            s,
            o ?? "NO REL NAME",
            a
          );
      }
    );
  }
  /**
   * Classifies fragments based on their spatial structure in the IFC model.
   *
   * @param model - The FragmentsGroup containing the fragments to be classified.
   * @param config - The configuration for the classifier. It includes "useProperties", which is true by default
   * (if false, the classification will use the expressIDs instead of the names), and "isolate", which will make
   * the classifier just pick the WEBIFC categories provided.
   *
   * @remarks
   * This method iterates through the relations of the fragments in the provided group,
   * and classifies them based on their spatial structure in the IFC model.
   * The classification is stored in the `list` property under the system name "spatialStructures",
   * with the relationship name as the class name and a map of fragment IDs to their respective express IDs as the value.
   *
   * @throws Will throw an error if the fragment ID is not found or if the model relations do not exist.
   */
  async bySpatialStructure(t, e = {}) {
    var l, h;
    const s = this.components.get(Gt), n = s.relationMaps[t.uuid];
    if (!n)
      throw new Error(
        `Classifier: model relations of ${t.name || t.uuid} have to exists to group by spatial structure.`
      );
    const r = e.systemName ?? "spatialStructures", a = e.useProperties === void 0 || e.useProperties;
    for (const [f] of n) {
      if (e.isolate) {
        const E = t.data.get(f);
        if (!E)
          continue;
        const T = E[1][1];
        if (T === void 0 || !e.isolate.has(T))
          continue;
      }
      const I = s.getEntityRelations(
        t,
        f,
        "Decomposes"
      );
      if (I)
        for (const E of I) {
          let T = E.toString();
          if (a) {
            const p = await t.getProperties(E);
            if (!p)
              continue;
            T = (l = p.Name) == null ? void 0 : l.value;
          }
          this.saveItem(t, r, T, f, E);
        }
      const u = s.getEntityRelations(
        t,
        f,
        "ContainsElements"
      );
      if (!u)
        continue;
      let d = f.toString();
      if (a) {
        const E = await t.getProperties(f);
        if (!E)
          continue;
        d = (h = E.Name) == null ? void 0 : h.value;
      }
      for (const E of u) {
        this.saveItem(t, r, d, E, f);
        const T = s.getEntityRelations(
          t,
          Number(E),
          "IsDecomposedBy"
        );
        if (T)
          for (const p of T)
            this.saveItem(t, r, d, p, f);
      }
    }
  }
  /**
   * Sets the color of the specified fragments.
   *
   * @param items - A map of fragment IDs to their respective express IDs.
   * @param color - The color to set for the fragments.
   * @param override - A boolean indicating whether to override the existing color of the fragments.
   *
   * @remarks
   * This method iterates through the provided fragment IDs, retrieves the corresponding fragments,
   * and sets their color using the `setColor` method of the FragmentsGroup class.
   *
   * @throws Will throw an error if the fragment with the specified ID is not found.
   */
  setColor(t, e, s = !1) {
    const n = this.components.get(Tt);
    for (const r in t) {
      const o = n.list.get(r);
      if (!o)
        continue;
      const a = t[r];
      o.setColor(e, a, s);
    }
  }
  /**
   * Resets the color of the specified fragments to their original color.
   *
   * @param items - A map of fragment IDs to their respective express IDs.
   *
   * @remarks
   * This method iterates through the provided fragment IDs, retrieves the corresponding fragments,
   * and resets their color using the `resetColor` method of the FragmentsGroup class.
   *
   * @throws Will throw an error if the fragment with the specified ID is not found.
   */
  resetColor(t) {
    const e = this.components.get(Tt);
    for (const s in t) {
      const n = e.list.get(s);
      if (!n)
        continue;
      const r = t[s];
      n.resetColor(r);
    }
  }
  saveItem(t, e, s, n, r = null) {
    this.list[e] || (this.list[e] = {});
    const o = t.data.get(n);
    if (o)
      for (const a of o[0]) {
        const l = t.keyFragments.get(a);
        if (l) {
          const h = this.list[e];
          h[s] || (h[s] = { map: {}, id: r, name: s }), h[s].map[l] || (h[s].map[l] = /* @__PURE__ */ new Set()), h[s].map[l].add(n);
        }
      }
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(bs, "uuid", "e25a7f3c-46c4-4a14-9d3d-5115f24ebeb7");
let Bi = bs;
const vs = class vs extends At {
  constructor(t) {
    super(t);
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    /**
     * The height of the explosion animation.
     * This property determines the vertical distance by which fragments are moved during the explosion.
     * Default value is 10.
     */
    C(this, "height", 10);
    /**
     * The group name used for the explosion animation.
     * This property specifies the group of fragments that will be affected by the explosion.
     * Default value is "storeys".
     */
    C(this, "groupName", "spatialStructures");
    /**
     * A set of strings representing the exploded items.
     * This set is used to keep track of which items have been exploded.
     */
    C(this, "list", /* @__PURE__ */ new Set());
    t.add(vs.uuid, this);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.list.clear(), this.onDisposed.trigger(), this.onDisposed.reset();
  }
  /**
   * Sets the explosion state of the fragments.
   *
   * @param active - A boolean indicating whether to activate or deactivate the explosion.
   *
   * @remarks
   * This method applies a vertical transformation to the fragments based on the `active` parameter.
   * If `active` is true, the fragments are moved upwards by a distance determined by the `height` property.
   * If `active` is false, the fragments are moved back to their original position.
   *
   * The method also keeps track of the exploded items using the `list` set.
   *
   * @throws Will throw an error if the `Classifier` or `FragmentsManager` components are not found in the `components` system.
   */
  set(t) {
    if (!this.enabled)
      return;
    const e = this.components.get(Bi), s = this.components.get(Tt), n = t ? 1 : -1;
    let r = 0;
    const o = e.list[this.groupName], a = new D.Matrix4();
    for (const l in o) {
      a.elements[13] = r * n * this.height;
      for (const h in o[l].map) {
        const f = s.list.get(h), I = l + h, u = this.list.has(I);
        if (!f || t && u || !t && !u)
          continue;
        t ? this.list.add(I) : this.list.delete(I);
        const d = o[l].map[h];
        f.applyTransform(d, a), f.mesh.computeBoundingSphere(), f.mesh.computeBoundingBox();
      }
      r++;
    }
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(vs, "uuid", "d260618b-ce88-4c7d-826c-6debb91de3e2");
let Hr = vs;
const Us = class Us extends At {
  constructor(t) {
    super(t);
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    this.components.add(Us.uuid, this);
  }
  /**
   * Sets the visibility of fragments within the 3D scene.
   * If no `items` parameter is provided, all fragments will be set to the specified visibility.
   * If `items` is provided, only the specified fragments will be affected.
   *
   * @param visible - The visibility state to set for the fragments.
   * @param items - An optional map of fragment IDs and their corresponding sub-fragment IDs to be affected.
   * If not provided, all fragments will be affected.
   *
   * @returns {void}
   */
  set(t, e) {
    const s = this.components.get(Tt);
    if (!e) {
      for (const [n, r] of s.list)
        r && (r.setVisibility(t), this.updateCulledVisibility(r));
      return;
    }
    for (const n in e) {
      const r = e[n], o = s.list.get(n);
      o && (o.setVisibility(t, r), this.updateCulledVisibility(o));
    }
  }
  /**
   * Isolates fragments within the 3D scene by hiding all other fragments and showing only the specified ones.
   * It calls the `set` method twice: first to hide all fragments, and then to show only the specified ones.
   *
   * @param items - A map of fragment IDs and their corresponding sub-fragment IDs to be isolated.
   * If not provided, all fragments will be isolated.
   *
   * @returns {void}
   */
  isolate(t) {
    this.set(!1), this.set(!0, t);
  }
  updateCulledVisibility(t) {
    const e = this.components.get(Sn);
    for (const [s, n] of e.list) {
      const r = n.colorMeshes.get(t.id);
      r && (r.count = t.mesh.count);
    }
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(Us, "uuid", "dd9ccf2d-8a21-4821-b7f6-2949add16a29");
let Dn = Us;
class Jl extends zn {
  constructor() {
    super(...arguments);
    /**
     * Minimum number of geometries to be streamed.
     * Defaults to 10 geometries.
     */
    C(this, "minGeometrySize", 10);
    /**
     * Minimum amount of assets to be streamed.
     * Defaults to 1000 assets.
     */
    C(this, "minAssetsSize", 1e3);
    /**
     * Maximum amount of triangles per fragment. Useful for controlling the maximum size of fragment files.
     */
    C(this, "maxTriangles", null);
  }
}
const xs = class xs extends At {
  constructor(t) {
    super(t);
    /**
     * Event triggered when geometry is streamed.
     * Contains the streamed geometry data and its buffer.
     */
    C(this, "onGeometryStreamed", new _e());
    /**
     * Event triggered when assets are streamed.
     * Contains the streamed assets.
     */
    C(this, "onAssetStreamed", new _e());
    /**
     * Event triggered to indicate the progress of the streaming process.
     * Contains the progress percentage.
     */
    C(this, "onProgress", new _e());
    /**
     * Event triggered when the IFC file is loaded.
     * Contains the loaded IFC file data.
     */
    C(this, "onIfcLoaded", new _e());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /**
     * Settings for the IfcGeometryTiler.
     */
    C(this, "settings", new Jl());
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    /**
     * The WebIFC API instance used for IFC file processing.
     */
    C(this, "webIfc", new k.IfcAPI());
    C(this, "_nextAvailableID", 0);
    C(this, "_splittedGeometries", /* @__PURE__ */ new Map());
    C(this, "_spatialTree", new ro());
    C(this, "_metaData", new ao());
    C(this, "_visitedGeometries", /* @__PURE__ */ new Map());
    C(this, "_streamSerializer", new Qt.StreamSerializer());
    C(this, "_geometries", /* @__PURE__ */ new Map());
    C(this, "_geometryCount", 0);
    C(this, "_civil", new oo());
    C(this, "_groupSerializer", new Qt.Serializer());
    C(this, "_assets", []);
    C(this, "_meshesWithHoles", /* @__PURE__ */ new Set());
    this.components.add(xs.uuid, this), this.settings.excludedCategories.add(k.IFCOPENINGELEMENT);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.onIfcLoaded.reset(), this.onGeometryStreamed.reset(), this.onAssetStreamed.reset(), this.webIfc = null, this.onDisposed.trigger(), this.onDisposed.reset();
  }
  /**
   * This method streams the IFC file from a given buffer.
   *
   * @param data - The Uint8Array containing the IFC file data.
   * @returns A Promise that resolves when the streaming process is complete.
   *
   * @remarks
   * This method cleans up any resources after the streaming process is complete.
   *
   * @example
   * ```typescript
   * const ifcData = await fetch('path/to/ifc/file.ifc');
   * const rawBuffer = await response.arrayBuffer();
   * const ifcBuffer = new Uint8Array(rawBuffer);
   * await ifcGeometryTiler.streamFromBuffer(ifcBuffer);
   * ```
   */
  async streamFromBuffer(t) {
    await this.readIfcFile(t), await this.streamAllGeometries(), this.cleanUp();
  }
  /**
   * This method streams the IFC file from a given callback.
   *
   * @param loadCallback - The callback function that will be used to load the IFC file.
   * @returns A Promise that resolves when the streaming process is complete.
   *
   * @remarks
   * This method cleans up any resources after the streaming process is complete.
   *
   */
  async streamFromCallBack(t) {
    await this.streamIfcFile(t), await this.streamAllGeometries(), this.cleanUp();
  }
  async readIfcFile(t) {
    const { path: e, absolute: s, logLevel: n } = this.settings.wasm;
    this.webIfc.SetWasmPath(e, s), await this.webIfc.Init(), n && this.webIfc.SetLogLevel(n), this.webIfc.OpenModel(t, this.settings.webIfc), this._nextAvailableID = this.webIfc.GetMaxExpressID(0);
  }
  async streamIfcFile(t) {
    const { path: e, absolute: s, logLevel: n } = this.settings.wasm;
    this.webIfc.SetWasmPath(e, s), await this.webIfc.Init(), n && this.webIfc.SetLogLevel(n), this.webIfc.OpenModelFromCallback(t, this.settings.webIfc);
  }
  async streamAllGeometries() {
    const { minGeometrySize: t, minAssetsSize: e } = this.settings;
    this._spatialTree.setUp(this.webIfc);
    const s = this.webIfc.GetIfcEntityList(0), n = [[]], r = new Qt.FragmentsGroup();
    r.ifcMetadata = {
      name: "",
      description: "",
      ...this._metaData.getNameInfo(this.webIfc),
      ...this._metaData.getDescriptionInfo(this.webIfc),
      schema: this.webIfc.GetModelSchema(0) || "IFC2X3",
      maxExpressID: this.webIfc.GetMaxExpressID(0)
    };
    let o = 0, a = 0;
    for (const E of s) {
      if (!this.webIfc.IsIfcElement(E) && E !== k.IFCSPACE || this.settings.excludedCategories.has(E))
        continue;
      const T = this.webIfc.GetLineIDsWithType(0, E), p = T.size();
      for (let R = 0; R < p; R++) {
        o > t && (o = 0, a++, n.push([]));
        const S = T.get(R);
        n[a].push(S);
        const m = this.webIfc.GetLine(0, S);
        if (m.GlobalId) {
          const O = (m == null ? void 0 : m.GlobalId.value) || (m == null ? void 0 : m.GlobalId);
          r.globalToExpressIDs.set(O, S);
        }
        const F = this._spatialTree.itemsByFloor[S] || 0;
        r.data.set(S, [[], [F, E]]), o++;
      }
    }
    this._spatialTree.cleanUp();
    let l = 0.01, h = 0;
    for (const E of n) {
      h++, this.webIfc.StreamMeshes(0, E, (p) => {
        this.getMesh(this.webIfc, p, r);
      }), this._geometryCount > this.settings.minGeometrySize && await this.streamGeometries(), this._assets.length > e && await this.streamAssets();
      const T = h / n.length;
      T > l && (l += 0.01, l = Math.max(l, T), await this.onProgress.trigger(Math.round(l * 100) / 100));
    }
    this._geometryCount && await this.streamGeometries(), this._assets.length && await this.streamAssets();
    const { opaque: f, transparent: I } = r.geometryIDs;
    for (const [E, { index: T, uuid: p }] of this._visitedGeometries)
      r.keyFragments.set(T, p), (E > 1 ? f : I).set(E, T);
    co.get(r, this.webIfc);
    const u = this.webIfc.GetCoordinationMatrix(0);
    r.coordinationMatrix.fromArray(u), r.civilData = this._civil.read(this.webIfc);
    const d = this._groupSerializer.export(r);
    await this.onIfcLoaded.trigger(d), r.dispose(!0);
  }
  cleanUp() {
    try {
      this.webIfc.Dispose();
    } catch {
    }
    this.webIfc = null, this.webIfc = new k.IfcAPI(), this._visitedGeometries.clear(), this._geometries.clear(), this._assets = [], this._meshesWithHoles.clear();
  }
  getMesh(t, e, s) {
    const n = e.geometries.size(), r = e.expressID, o = { id: r, geometries: [] };
    for (let a = 0; a < n; a++) {
      const l = e.geometries.get(a), h = l.geometryExpressID, I = l.color.w === 1 ? 1 : -1, u = h * I;
      this._visitedGeometries.has(u) || this.getGeometry(t, h, u), this.registerGeometryData(
        s,
        r,
        l,
        o,
        h,
        u
      );
      const d = this._splittedGeometries.get(h);
      if (d)
        for (const E of d)
          this.registerGeometryData(s, r, l, o, E, E);
    }
    this._assets.push(o);
  }
  getGeometry(t, e, s) {
    const n = t.GetGeometry(0, e), r = t.GetIndexArray(
      n.GetIndexData(),
      n.GetIndexDataSize()
    ), o = t.GetVertexArray(
      n.GetVertexData(),
      n.GetVertexDataSize()
    ), a = new Float32Array(o.length / 2), l = new Float32Array(o.length / 2);
    for (let u = 0; u < o.length; u += 6)
      a[u / 2] = o[u], a[u / 2 + 1] = o[u + 1], a[u / 2 + 2] = o[u + 2], l[u / 2] = o[u + 3], l[u / 2 + 1] = o[u + 4], l[u / 2 + 2] = o[u + 5];
    if (r.length === 0) {
      const u = new Float32Array([
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        1
      ]);
      this._geometries.set(e, {
        position: a,
        normal: l,
        index: r,
        boundingBox: u,
        hasHoles: !1
      });
      const d = this._visitedGeometries.size, E = D.MathUtils.generateUUID();
      this._visitedGeometries.set(s, { uuid: E, index: d }), this._geometryCount++, n.delete();
      return;
    }
    const f = (this.settings.maxTriangles || r.length / 3) * 3;
    let I = !0;
    for (let u = 0; u < r.length; u += f) {
      const d = r.length - u, E = Math.min(d, f), T = u + E, p = [], R = [], S = [];
      let m = 0;
      for (let g = u; g < T; g++) {
        p.push(m++);
        const v = r[g];
        R.push(a[v * 3]), R.push(a[v * 3 + 1]), R.push(a[v * 3 + 2]), S.push(l[v * 3]), S.push(l[v * 3 + 1]), S.push(l[v * 3 + 2]);
      }
      const F = new Uint32Array(p), O = new Float32Array(R), y = new Float32Array(S), w = qa(O), L = new Float32Array(w.transformation.elements), b = !1, Y = I ? e : this._nextAvailableID++;
      this._geometries.set(Y, {
        position: O,
        normal: y,
        index: F,
        boundingBox: L,
        hasHoles: b
      }), I || (this._splittedGeometries.has(e) || this._splittedGeometries.set(e, /* @__PURE__ */ new Set()), this._splittedGeometries.get(e).add(Y));
      const N = this._visitedGeometries.size, M = D.MathUtils.generateUUID();
      this._visitedGeometries.set(s, { uuid: M, index: N }), this._geometryCount++, I = !1;
    }
    n.delete();
  }
  async streamAssets() {
    await this.onAssetStreamed.trigger(this._assets), this._assets = null, this._assets = [];
  }
  async streamGeometries() {
    let t = this._streamSerializer.export(this._geometries), e = {};
    for (const [s, { boundingBox: n, hasHoles: r }] of this._geometries)
      e[s] = { boundingBox: n, hasHoles: r };
    await this.onGeometryStreamed.trigger({ data: e, buffer: t }), e = null, t = null, this._geometries.clear(), this._geometryCount = 0;
  }
  registerGeometryData(t, e, s, n, r, o) {
    const a = this._visitedGeometries.get(o);
    if (a === void 0)
      throw new Error("Error getting geometry data for streaming!");
    const l = t.data.get(e);
    if (!l)
      throw new Error("Data not found!");
    l[0].push(a.index);
    const { x: h, y: f, z: I, w: u } = s.color, d = [h, f, I, u], E = s.flatTransformation;
    n.geometries.push({ color: d, geometryID: r, transformation: E });
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(xs, "uuid", "d9999a00-e1f5-4d3f-8cfe-c56e08609764");
let Wr = xs;
class th extends zn {
  constructor() {
    super(...arguments);
    /**
     * Amount of properties to be streamed.
     * Defaults to 100 properties.
     */
    C(this, "propertiesSize", 100);
  }
}
class eh extends At {
  constructor() {
    super(...arguments);
    /**
     * An event that is triggered when properties are streamed from the IFC file.
     * The event provides the type of the IFC entity and the corresponding data.
     */
    C(this, "onPropertiesStreamed", new _e());
    /**
     * An event that is triggered to indicate the progress of the streaming process.
     * The event provides a number between 0 and 1 representing the progress percentage.
     */
    C(this, "onProgress", new _e());
    /**
     * An event that is triggered when indices are streamed from the IFC file.
     * The event provides a map of indices, where the key is the entity type and the value is another map of indices.
     */
    C(this, "onIndicesStreamed", new _e());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    /**
     * An instance of the PropertiesStreamingSettings class, which holds the settings for the streaming process.
     */
    C(this, "settings", new th());
    /**
     * An instance of the IfcAPI class from the Web-IFC library, which provides methods for reading and processing IFC data.
     */
    C(this, "webIfc", new k.IfcAPI());
  }
  /** {@link Disposable.dispose} */
  async dispose() {
    this.onIndicesStreamed.reset(), this.onPropertiesStreamed.reset(), this.webIfc = null, this.onDisposed.reset();
  }
  /**
   * This method converts properties from an IFC file to tiles given its data as a Uint8Array.
   *
   * @param data - The Uint8Array containing the IFC file data.
   * @returns A Promise that resolves when the streaming process is complete.
   */
  async streamFromBuffer(t) {
    await this.readIfcFile(t), await this.streamAllProperties(), this.cleanUp();
  }
  /**
   * This method converts properties from an IFC file to tiles using a given callback function to read the file.
   *
   * @param loadCallback - A callback function that loads the IFC file data.
   * @returns A Promise that resolves when the streaming process is complete.
   */
  async streamFromCallBack(t) {
    await this.streamIfcFile(t), await this.streamAllProperties(), this.cleanUp();
  }
  async readIfcFile(t) {
    const { path: e, absolute: s, logLevel: n } = this.settings.wasm;
    this.webIfc.SetWasmPath(e, s), await this.webIfc.Init(), n && this.webIfc.SetLogLevel(n), this.webIfc.OpenModel(t, this.settings.webIfc);
  }
  async streamIfcFile(t) {
    const { path: e, absolute: s, logLevel: n } = this.settings.wasm;
    this.webIfc.SetWasmPath(e, s), await this.webIfc.Init(), n && this.webIfc.SetLogLevel(n), this.webIfc.OpenModelFromCallback(t, this.settings.webIfc);
  }
  async streamAllProperties() {
    const { propertiesSize: t } = this.settings, e = new Set(this.webIfc.GetIfcEntityList(0)), s = /* @__PURE__ */ new Set([
      k.IFCPROJECT,
      k.IFCSITE,
      k.IFCBUILDING,
      k.IFCBUILDINGSTOREY,
      k.IFCSPACE
    ]);
    for (const l of s)
      e.add(l);
    let n = 0.01, r = 0;
    for (const l of e) {
      if (r++, lo.has(l))
        continue;
      const h = s.has(l), f = this.webIfc.GetLineIDsWithType(0, l), I = f.size();
      let u = 0;
      for (let E = 0; E < I - t; E += t) {
        const T = {};
        for (let p = 0; p < t; p++) {
          u++;
          const R = f.get(E + p);
          try {
            const S = this.webIfc.GetLine(0, R, h);
            T[S.expressID] = S;
          } catch {
            console.log(`Could not get property: ${R}`);
          }
        }
        await this.onPropertiesStreamed.trigger({ type: l, data: T });
      }
      if (u !== I) {
        const E = {};
        for (let T = u; T < I; T++) {
          const p = f.get(T);
          try {
            const R = this.webIfc.GetLine(0, p, h);
            E[R.expressID] = R;
          } catch {
            console.log(`Could not get property: ${p}`);
          }
        }
        await this.onPropertiesStreamed.trigger({ type: l, data: E });
      }
      r / e.size > n && (n = Math.round(n * 100) / 100, await this.onProgress.trigger(n), n += 0.01);
    }
    await this.onProgress.trigger(1);
    const a = await this.components.get(Gt).processFromWebIfc(this.webIfc, 0);
    await this.onIndicesStreamed.trigger(a);
  }
  cleanUp() {
    this.webIfc.Dispose(), this.webIfc = null, this.webIfc = new k.IfcAPI();
  }
}
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(eh, "uuid", "88d2c89c-ce32-47d7-8cb6-d51e4b311a0b");
class mo {
  constructor(i, t, e) {
    C(this, "title");
    C(this, "guid", oe.create());
    /**
     * ClippingPlanes can be used to define a subsection of a building model that is related to the topic.
     * Each clipping plane is defined by Location and Direction.
     * The Direction vector points in the invisible direction meaning the half-space that is clipped.
     * @experimental
     */
    C(this, "clippingPlanes", new we());
    C(this, "camera", {
      aspectRatio: 1,
      fov: 60,
      direction: { x: 0, y: 0, z: 0 },
      position: { x: 0, y: 0, z: 0 }
    });
    /**
     * A list of components GUIDs to hide when defaultVisibility = true or to show when defaultVisibility = false
     */
    C(this, "exceptionComponents", new we());
    /**
     * A list of components GUIDs that should be selected (highlighted) when displaying a viewpoint.
     */
    C(this, "selectionComponents", new we());
    /**
     * A map of colors and components GUIDs that should be colorized when displaying a viewpoint.
     * For this to work, call viewpoint.colorize()
     */
    C(this, "componentColors", new re());
    /**
     * Boolean flags to allow fine control over the visibility of spaces.
     * A typical use of these flags is when DefaultVisibility=true but spaces should remain hidden.
     * @default false
     */
    C(this, "spacesVisible", !1);
    /**
     * Boolean flags to allow fine control over the visibility of space boundaries.
     * A typical use of these flags is when DefaultVisibility=true but space boundaries should remain hidden.
     * @default false
     */
    C(this, "spaceBoundariesVisible", !1);
    /**
     * Boolean flags to allow fine control over the visibility of openings.
     * A typical use of these flags is when DefaultVisibility=true but openings should remain hidden.
     * @default false
     */
    C(this, "openingsVisible", !1);
    /**
     * When true, all components should be visible unless listed in the exceptions
     * When false all components should be invisible unless listed in the exceptions
     */
    C(this, "defaultVisibility", !0);
    C(this, "_components");
    /**
     * Represents the world in which the viewpoints are created and managed.
     */
    C(this, "world");
    const s = { setCamera: !0, ...e }, { data: n, setCamera: r } = s;
    this._components = i, this.world = t, n && (this.guid = n.guid ?? this.guid, this.set(n)), r && this.updateCamera();
  }
  get _selectionModelIdMap() {
    const i = this._components.get(Tt), t = {};
    for (const [e, s] of i.groups) {
      e in t || (t[e] = /* @__PURE__ */ new Set());
      for (const n of this.selectionComponents) {
        const r = s.globalToExpressIDs.get(n);
        r && t[e].add(r);
      }
    }
    return t;
  }
  get _exceptionModelIdMap() {
    const i = this._components.get(Tt), t = {};
    for (const [e, s] of i.groups) {
      e in t || (t[e] = /* @__PURE__ */ new Set());
      for (const n of this.exceptionComponents) {
        const r = s.globalToExpressIDs.get(n);
        r && t[e].add(r);
      }
    }
    return t;
  }
  /**
   * A list of components that should be selected (highlighted) when displaying a viewpoint.
   * @returns The fragmentIdMap for components marked as selections.
   */
  get selection() {
    return this._components.get(Tt).modelIdToFragmentIdMap(
      this._selectionModelIdMap
    );
  }
  /**
   * A list of components to hide when defaultVisibility = true or to show when defaultVisibility = false
   * @returns The fragmentIdMap for components marked as exceptions.
   */
  get exception() {
    return this._components.get(Tt).modelIdToFragmentIdMap(
      this._exceptionModelIdMap
    );
  }
  /**
   * Retrieves the projection type of the viewpoint's camera.
   *
   * @returns A string representing the projection type of the viewpoint's camera.
   *          It can be either 'Perspective' or 'Orthographic'.
   */
  get projection() {
    return "fov" in this.camera ? "Perspective" : "Orthographic";
  }
  /**
   * Retrieves the position vector of the viewpoint's camera.
   *
   * @remarks
   * The position vector represents the camera's position in the world coordinate system.
   * The function applies the base coordinate system transformation to the position vector.
   *
   * @returns A THREE.Vector3 representing the position of the viewpoint's camera.
   */
  get position() {
    const i = this._components.get(Tt), { position: t } = this.camera, { x: e, y: s, z: n } = t, r = new D.Vector3(e, s, n);
    return i.applyBaseCoordinateSystem(r, new D.Matrix4()), r;
  }
  /**
   * Retrieves the direction vector of the viewpoint's camera.
   *
   * @remarks
   * The direction vector represents the direction in which the camera is pointing.
   * It is calculated by extracting the x, y, and z components from the camera's direction property.
   *
   * @returns A THREE.Vector3 representing the direction of the viewpoint's camera.
   */
  get direction() {
    const { direction: i } = this.camera, { x: t, y: e, z: s } = i;
    return new D.Vector3(t, e, s);
  }
  get _managerVersion() {
    return this._components.get(Mt).config.version;
  }
  /**
   * Retrieves the list of BCF topics associated with the current viewpoint.
   *
   * @remarks
   * This function retrieves the BCFTopics manager from the components,
   * then filters the list of topics to find those associated with the current viewpoint.
   *
   * @returns An array of BCF topics associated with the current viewpoint.
   */
  get topics() {
    return [...this._components.get(Mt).list.values()].filter(
      (s) => s.viewpoints.has(this.guid)
    );
  }
  /**
   * Adds components to the viewpoint based on the provided fragment ID map.
   *
   * @param fragmentIdMap - A map containing fragment IDs as keys and arrays of express IDs as values.
   */
  addComponentsFromMap(i) {
    const e = this._components.get(Tt).fragmentIdMapToGuids(i);
    this.selectionComponents.add(...e), this._components.get(ie).list.set(this.guid, this);
  }
  /**
   * Replace the properties of the viewpoint with the provided data.
   *
   * @remarks The guid will be ommited as it shouldn't change after it has been initially set.
   * @remarks The existing selection and exception components will be fully replaced in case new ones are provided.
   *
   * @param data - An object containing the properties to be set.
   *               The properties not included in the object will remain unchanged.
   *
   * @returns The viewpoint instance with the updated properties.
   */
  set(i) {
    const t = i, e = this;
    for (const n in i) {
      if (n === "guid")
        continue;
      const r = t[n];
      if (n === "selectionComponents") {
        this.selectionComponents.clear(), this.selectionComponents.add(...r);
        continue;
      }
      if (n === "exceptionComponents") {
        this.exceptionComponents.clear(), this.exceptionComponents.add(...r);
        continue;
      }
      n in this && (e[n] = r);
    }
    return this._components.get(ie).list.set(this.guid, this), this;
  }
  /**
   * Sets the viewpoint of the camera in the world.
   *
   * @remarks
   * This function calculates the target position based on the viewpoint information.
   * It sets the visibility of the viewpoint components and then applies the viewpoint using the camera's controls.
   *
   * @param transition - Indicates whether the camera movement should have a transition effect.
   *                      Default value is `true`.
   *
   * @throws An error if the world's camera does not have camera controls.
   *
   * @returns A Promise that resolves when the camera has been set.
   */
  async go(i = !0) {
    const { camera: t } = this.world;
    if (!t.hasCameraControls())
      throw new Error(
        "Viewpoint: the world's camera need controls to set the viewpoint."
      );
    t instanceof Pc && t.projection.set(this.projection);
    const e = new D.Vector3(
      this.camera.position.x,
      this.camera.position.y,
      this.camera.position.z
    ), s = new D.Vector3(
      this.camera.direction.x,
      this.camera.direction.y,
      this.camera.direction.z
    );
    if (e.equals(new D.Vector3()) && s.equals(new D.Vector3()))
      return;
    const n = this.position, r = this.direction;
    let o = {
      x: n.x + r.x * 80,
      y: n.y + r.y * 80,
      z: n.z + r.z * 80
    };
    const a = this.selection;
    if (Object.keys(a).length === 0) {
      const f = this._components.get(ci).get(this.world).castRayFromVector(n, this.direction);
      f && (o = f.point);
    } else {
      const l = this._components.get(Mn);
      l.reset(), l.addFragmentIdMap(a), o = l.getSphere().center, l.reset();
    }
    await t.controls.setLookAt(
      n.x,
      n.y,
      n.z,
      o.x,
      o.y,
      o.z,
      i
    );
  }
  /**
   * Updates the camera settings of the viewpoint based on the current world's camera and renderer.
   *
   * @remarks
   * This function retrieves the camera's position, direction, and aspect ratio from the world's camera and renderer.
   * It then calculates the camera's perspective or orthographic settings based on the camera type.
   * Finally, it updates the viewpoint's camera settings and updates the viewpoint to the Viewpoints manager.
   *
   * @throws An error if the world's camera does not have camera controls.
   * @throws An error if the world's renderer is not available.
   */
  updateCamera() {
    const { camera: i, renderer: t } = this.world;
    if (!t)
      throw new Error("Viewpoint: the world needs to have a renderer!");
    if (!i.hasCameraControls())
      throw new Error("Viewpoint: world's camera need camera controls!");
    const e = new D.Vector3();
    i.controls.getPosition(e);
    const s = i.three, n = new D.Vector3(0, 0, -1).applyEuler(
      s.rotation
    ), { width: r, height: o } = t.getSize();
    let a = r / o;
    Number.isNaN(a) && (a = 1);
    const l = this._components.get(Tt);
    e.applyMatrix4(l.baseCoordinationMatrix.clone().invert());
    const h = {
      aspectRatio: a,
      position: { x: e.x, y: e.y, z: e.z },
      direction: { x: n.x, y: n.y, z: n.z }
    };
    s instanceof D.PerspectiveCamera ? this.camera = {
      ...h,
      fov: s.fov
    } : s instanceof D.OrthographicCamera && (this.camera = {
      ...h,
      viewToWorldScale: s.top - s.bottom
    }), this._components.get(ie).list.set(this.guid, this);
  }
  applyVisibility() {
    const i = this._components.get(Dn);
    i.set(this.defaultVisibility), i.set(!this.defaultVisibility, this.exception), i.set(!0, this.selection);
  }
  /**
   * Applies color to the components in the viewpoint based on their GUIDs.
   *
   * This function iterates through the `componentColors` map, retrieves the fragment IDs
   * corresponding to each color, and then uses the `Classifier` to apply the color to those fragments.
   *
   * @remarks
   * The color is applied using the `Classifier.setColor` method, which sets the color of the specified fragments.
   * The color is provided as a hexadecimal string, prefixed with a '#'.
   */
  applyColors() {
    const i = this._components.get(ie), t = this._components.get(Tt), e = this._components.get(Bi);
    for (const [s, n] of this.componentColors) {
      const r = t.guidToFragmentIdMap(n);
      e.setColor(r, s, i.config.overwriteColors);
    }
  }
  /**
   * Resets the colors of all components in the viewpoint to their original color.
   */
  resetColors() {
    const i = this._components.get(Tt), t = this._components.get(Bi);
    for (const [e, s] of this.componentColors) {
      const n = i.guidToFragmentIdMap(s);
      t.resetColor(n);
    }
  }
  async createComponentTags(i) {
    var n, r;
    const t = this._components.get(Tt), e = this._components.get(Mt);
    let s = "";
    if (e.config.includeSelectionTag) {
      const o = i === "selection" ? this._selectionModelIdMap : this._exceptionModelIdMap;
      for (const a in o) {
        const l = t.groups.get(a);
        if (!l)
          continue;
        const h = o[a];
        for (const f of h) {
          const I = await l.getProperties(f);
          if (!I)
            continue;
          const u = (n = I.GlobalId) == null ? void 0 : n.value;
          if (!u)
            continue;
          const d = (r = I.Tag) == null ? void 0 : r.value;
          let E = null;
          d && (E = `AuthoringToolId="${d}"`), s += `
<Component IfcGuid="${u}" ${E ?? ""} />`;
        }
      }
    } else
      s = [...this.selectionComponents].map((o) => `<Component IfcGuid="${o}" />`).join(`
`);
    return s;
  }
  createColorTags() {
    let i = "";
    for (const [t, e] of this.componentColors.entries()) {
      const s = `#${t.getHexString()}`, n = e.map((r) => `
<Component IfcGuid="${r}" />`).join(`
`);
      i += `<Color Color="${s}">
${n}
</Color>`;
    }
    return i.length !== 0 ? `<Coloring>
${i}
</Coloring>` : "<Coloring />";
  }
  /**
   * Serializes the viewpoint into a buildingSMART compliant XML string for export.
   *
   * @param version - The version of the BCF Manager to use for serialization.
   *                   If not provided, the current version of the manager will be used.
   *
   * @returns A Promise that resolves to an XML string representing the viewpoint.
   *          The XML string follows the BCF VisualizationInfo schema.
   *
   * @throws An error if the world's camera does not have camera controls.
   * @throws An error if the world's renderer is not available.
   */
  async serialize(i = this._managerVersion) {
    const t = this._components.get(Tt), e = this.position;
    e.applyMatrix4(t.baseCoordinationMatrix.clone().invert());
    const s = this.direction;
    s.normalize();
    const n = new D.Matrix4().makeRotationX(Math.PI / 2), r = s.clone().applyMatrix4(n);
    r.normalize();
    const o = `<CameraViewPoint>
      <X>${e.x}</X>
      <Y>${-e.z}</Y>
      <Z>${e.y}</Z>
    </CameraViewPoint>`, a = `<CameraDirection>
      <X>${s.x}</X>
      <Y>${-s.z}</Y>
      <Z>${s.y}</Z>
    </CameraDirection>`, l = `<CameraUpVector>
      <X>${r.x}</X>
      <Y>${-r.z}</Y>
      <Z>${r.y}</Z>
    </CameraUpVector>`, h = `<AspectRatio>${this.camera.aspectRatio}</AspectRatio>`;
    let f = "";
    "viewToWorld" in this.camera ? f = `<OrthogonalCamera>
        ${o}
        ${a}
        ${l}
        ${h}
        <ViewToWorldScale>${this.camera.viewToWorld}</ViewToWorldScale>
      </OrthogonalCamera>` : "fov" in this.camera && (f = `<PerspectiveCamera>
        ${o}
        ${a}
        ${l}
        ${h}
        <FieldOfView>${this.camera.fov}</FieldOfView>
      </PerspectiveCamera>`);
    const I = `<ViewSetupHints SpacesVisible="${this.spacesVisible ?? !1}" SpaceBoundariesVisible="${this.spaceBoundariesVisible ?? !1}" OpeningsVisible="${this.openingsVisible ?? !1}" />`, u = (await this.createComponentTags("selection")).trim(), d = (await this.createComponentTags("exception")).trim(), E = this.createColorTags();
    return `<?xml version="1.0" encoding="UTF-8"?>
    <VisualizationInfo Guid="${this.guid}">
      <Components>
        ${i === "2.1" ? I : ""}
        ${u.length !== 0 ? `<Selection>${u}</Selection>` : ""}
        <Visibility DefaultVisibility="${this.defaultVisibility}">
          ${i === "3" ? I : ""}
          ${d.length !== 0 ? `<Exceptions>${d}</Exceptions>` : ""}
        </Visibility>
        ${E}
      </Components>
      ${f}
    </VisualizationInfo>`;
  }
}
class ih extends Ge {
  constructor() {
    super(...arguments);
    C(this, "_config", {
      overwriteColors: {
        value: !1,
        type: "Boolean"
      }
    });
  }
  get overwriteColors() {
    return this._config.overwriteColors.value;
  }
  set overwriteColors(t) {
    this._config.overwriteColors.value = t;
  }
}
const Ui = class Ui extends At {
  constructor(t) {
    super(t);
    C(this, "enabled", !0);
    /**
     * A DataMap that stores Viewpoint instances, indexed by their unique identifiers (guid).
     * This map is used to manage and retrieve Viewpoint instances within the Viewpoints component.
     */
    C(this, "list", new re());
    C(this, "isSetup", !1);
    C(this, "onSetup", new j());
    C(this, "config", new ih(
      this,
      this.components,
      "Viewpoints",
      Ui.uuid
    ));
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    t.add(Ui.uuid, this);
  }
  /**
   * Creates a new Viewpoint instance and adds it to the list.
   *
   * @param world - The world in which the Viewpoint will be created.
   * @param data - Optional partial data for the Viewpoint. If not provided, default data will be used.
   *
   * @returns The newly created Viewpoint instance.
   */
  create(t, e) {
    const s = new mo(this.components, t, { data: e });
    return e || this.list.set(s.guid, s), s;
  }
  setup() {
  }
  /**
   * Disposes of the Viewpoints component and its associated resources.
   *
   * This method is responsible for cleaning up any resources held by the Viewpoints component,
   * such as disposing of the DataMap of Viewpoint instances and triggering and resetting the
   * onDisposed event.
   */
  dispose() {
    this.list.dispose(), this.onDisposed.trigger(), this.onDisposed.reset();
  }
};
C(Ui, "uuid", "ee867824-a796-408d-8aa0-4e5962a83c66");
let ie = Ui;
class sh extends Ge {
  constructor() {
    super(...arguments);
    C(this, "_config", {
      visible: {
        value: !0,
        type: "Boolean"
      },
      lockRotation: {
        value: !0,
        type: "Boolean"
      },
      zoom: {
        type: "Number",
        interpolable: !0,
        value: 0.05,
        min: 1e-3,
        max: 5
      },
      frontOffset: {
        type: "Number",
        interpolable: !0,
        value: 0,
        min: 0,
        max: 100
      },
      sizeX: {
        type: "Number",
        interpolable: !0,
        value: 320,
        min: 20,
        max: 5e3
      },
      sizeY: {
        type: "Number",
        interpolable: !0,
        value: 160,
        min: 20,
        max: 5e3
      },
      backgroundColor: {
        value: new D.Color(),
        type: "Color"
      }
    });
  }
  /**
   * Whether the minimap is visible or not.
   */
  get visible() {
    return this._config.visible.value;
  }
  /**
   * Whether the minimap is visible or not.
   */
  set visible(t) {
    this._config.visible.value = t;
    const e = this._component.renderer.domElement.style;
    e.display = t ? "block" : "none";
  }
  /**
   * Whether to lock the rotation of the top camera in the minimap.
   */
  get lockRotation() {
    return this._config.lockRotation.value;
  }
  /**
   * Whether to lock the rotation of the top camera in the minimap.
   */
  set lockRotation(t) {
    this._config.lockRotation.value = t, this._component.lockRotation = t;
  }
  /**
   * The zoom of the camera in the minimap.
   */
  get zoom() {
    return this._config.zoom.value;
  }
  /**
   * The zoom of the camera in the minimap.
   */
  set zoom(t) {
    this._config.zoom.value = t, this._component.zoom = t;
  }
  /**
   * The front offset of the minimap.
   * It determines how much the minimap's view is offset from the camera's view.
   * By pushing the map to the front, what the user sees on screen corresponds with what they see on the map
   */
  get frontOffset() {
    return this._config.frontOffset.value;
  }
  /**
   * The front offset of the minimap.
   * It determines how much the minimap's view is offset from the camera's view.
   * By pushing the map to the front, what the user sees on screen corresponds with what they see on the map
   */
  set frontOffset(t) {
    this._config.frontOffset.value = t, this._component.frontOffset = t;
  }
  /**
   * The horizontal dimension of the minimap.
   */
  get sizeX() {
    return this._config.sizeX.value;
  }
  /**
   * The horizontal dimension of the minimap.
   */
  set sizeX(t) {
    this._config.sizeX.value = t;
    const { sizeX: e, sizeY: s } = this._config, n = new D.Vector2(e.value, s.value);
    this._component.resize(n);
  }
  /**
   * The vertical dimension of the minimap.
   */
  get sizeY() {
    return this._config.sizeY.value;
  }
  /**
   * The vertical dimension of the minimap.
   */
  set sizeY(t) {
    this._config.sizeY.value = t;
    const { sizeX: e, sizeY: s } = this._config, n = new D.Vector2(e.value, s.value);
    this._component.resize(n);
  }
  /**
   * The color of the background of the minimap.
   */
  get backgroundColor() {
    return this._config.backgroundColor.value;
  }
  /**
   * The color of the background of the minimap.
   */
  set backgroundColor(t) {
    this._config.backgroundColor.value = t, this._component.backgroundColor = t;
  }
}
class nh {
  constructor(i, t) {
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /** {@link Updateable.onAfterUpdate} */
    C(this, "onAfterUpdate", new j());
    /** {@link Updateable.onBeforeUpdate} */
    C(this, "onBeforeUpdate", new j());
    /** {@link Resizeable.onResize} */
    C(this, "onResize", new j());
    /** {@link Configurable.onSetup} */
    C(this, "onSetup", new j());
    /**
     * The front offset of the minimap.
     * It determines how much the minimap's view is offset from the camera's view.
     * By pushing the map to the front, what the user sees on screen corresponds with what they see on the map
     */
    C(this, "frontOffset", 0);
    /**
     * The override material for the minimap.
     * It is used to render the depth information of the world onto the minimap.
     */
    C(this, "overrideMaterial", new D.MeshDepthMaterial());
    /**
     * The background color of the minimap.
     * It is used to set the background color of the minimap's renderer.
     */
    C(this, "backgroundColor", new D.Color(395274));
    /**
     * The WebGL renderer for the minimap.
     * It is used to render the minimap onto the screen.
     */
    C(this, "renderer");
    /**
     * A flag indicating whether the minimap is enabled.
     * If disabled, the minimap will not update or render.
     */
    C(this, "enabled", !0);
    /**
     * The world in which the minimap is displayed.
     * It provides access to the 3D scene, camera, and other relevant world elements.
     */
    C(this, "world");
    /** {@link Configurable.config} */
    C(this, "config");
    /** {@link Configurable.isSetup} */
    C(this, "isSetup", !1);
    C(this, "_defaultConfig", {
      visible: !0,
      lockRotation: !1,
      zoom: 0.05,
      frontOffset: 0,
      sizeX: 320,
      sizeY: 160,
      backgroundColor: new D.Color(395274)
    });
    C(this, "_lockRotation", !0);
    C(this, "_size", new D.Vector2(320, 160));
    C(this, "_camera");
    C(this, "_plane");
    C(this, "_tempVector1", new D.Vector3());
    C(this, "_tempVector2", new D.Vector3());
    C(this, "_tempTarget", new D.Vector3());
    C(this, "down", new D.Vector3(0, -1, 0));
    C(this, "updatePlanes", () => {
      if (!this.world.renderer)
        throw new Error("The given world must have a renderer!");
      const i = [], t = this.world.renderer.three;
      for (const e of t.clippingPlanes)
        i.push(e);
      i.push(this._plane), this.renderer.clippingPlanes = i;
    });
    if (this.world = i, !this.world.renderer)
      throw new Error("The given world must have a renderer!");
    this.renderer = new D.WebGLRenderer(), this.renderer.setSize(this._size.x, this._size.y);
    const e = 1, s = this._size.x / this._size.y;
    this._camera = new D.OrthographicCamera(
      e * s / -2,
      e * s / 2,
      e / 2,
      e / -2
    ), this.world.renderer.onClippingPlanesUpdated.add(this.updatePlanes), this._camera.position.set(0, 200, 0), this._camera.zoom = 0.1, this._camera.rotation.x = -Math.PI / 2, this._plane = new D.Plane(this.down, 200), this.updatePlanes(), this.config = new sh(this, t, "MiniMap");
  }
  /**
   * Gets or sets whether the minimap rotation is locked.
   * When rotation is locked, the minimap will always face the same direction as the camera.
   */
  get lockRotation() {
    return this._lockRotation;
  }
  /**
   * Sets whether the minimap rotation is locked.
   * When rotation is locked, the minimap will always face the same direction as the camera.
   * @param active - If `true`, rotation is locked. If `false`, rotation is not locked.
   */
  set lockRotation(i) {
    this._lockRotation = i, i && (this._camera.rotation.z = 0);
  }
  /**
   * Gets the current zoom level of the minimap.
   * The zoom level determines how much of the world is visible on the minimap.
   * @returns The current zoom level of the minimap.
   */
  get zoom() {
    return this._camera.zoom;
  }
  /**
   * Sets the zoom level of the minimap.
   * The zoom level determines how much of the world is visible on the minimap.
   * @param value - The new zoom level of the minimap.
   */
  set zoom(i) {
    this._camera.zoom = i, this._camera.updateProjectionMatrix();
  }
  /** {@link Disposable.dispose} */
  dispose() {
    this.enabled = !1, this.onBeforeUpdate.reset(), this.onAfterUpdate.reset(), this.onResize.reset(), this.overrideMaterial.dispose(), this.renderer.forceContextLoss(), this.renderer.dispose(), this.onDisposed.trigger(), this.onDisposed.reset();
  }
  /** Returns the camera used by the MiniMap */
  get() {
    return this._camera;
  }
  /** {@link Updateable.update} */
  update() {
    if (!this.enabled)
      return;
    this.onBeforeUpdate.trigger();
    const i = this.world.scene.three, t = this.world.camera;
    if (!t.hasCameraControls())
      throw new Error("The given world must use camera controls!");
    if (!(i instanceof D.Scene))
      throw new Error("The given world must have a THREE.Scene as a root!");
    const e = t.controls;
    if (e.getPosition(this._tempVector1), this._camera.position.x = this._tempVector1.x, this._camera.position.z = this._tempVector1.z, this.frontOffset !== 0 && (e.getTarget(this._tempVector2), this._tempVector2.sub(this._tempVector1), this._tempVector2.normalize().multiplyScalar(this.frontOffset), this._camera.position.x += this._tempVector2.x, this._camera.position.z += this._tempVector2.z), !this._lockRotation) {
      e.getTarget(this._tempTarget);
      const n = Math.atan2(
        this._tempTarget.x - this._tempVector1.x,
        this._tempTarget.z - this._tempVector1.z
      );
      this._camera.rotation.z = n + Math.PI;
    }
    this._plane.set(this.down, this._tempVector1.y);
    const s = i.background;
    i.background = this.backgroundColor, this.renderer.render(i, this._camera), i.background = s, this.onAfterUpdate.trigger();
  }
  /** {@link Resizeable.getSize} */
  getSize() {
    return this._size;
  }
  /** {@link Resizeable.resize} */
  resize(i = this._size) {
    this._size.copy(i), this.renderer.setSize(i.x, i.y);
    const t = i.x / i.y, e = 1;
    this._camera.left = e * t / -2, this._camera.right = e * t / 2, this._camera.top = e / 2, this._camera.bottom = -e / 2, this._camera.updateProjectionMatrix(), this.onResize.trigger(i);
  }
  /** {@link Configurable.setup} */
  setup(i) {
    const t = { ...this._defaultConfig, ...i };
    this.config.visible = !0, this.config.lockRotation = t.lockRotation, this.config.zoom = t.zoom, this.config.frontOffset = t.frontOffset, this.config.sizeX = t.sizeX, this.config.sizeY = t.sizeY, this.config.backgroundColor = t.backgroundColor, this.isSetup = !0, this.onSetup.trigger();
  }
}
const Bs = class Bs extends At {
  constructor(t) {
    super(t);
    /** {@link Updateable.onAfterUpdate} */
    C(this, "onAfterUpdate", new j());
    /** {@link Updateable.onBeforeUpdate} */
    C(this, "onBeforeUpdate", new j());
    /** {@link Disposable.onDisposed} */
    C(this, "onDisposed", new j());
    /** {@link Configurable.onSetup} */
    C(this, "onSetup", new j());
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    /**
     * A collection of {@link MiniMap} instances, each associated with a unique world ID.
     */
    C(this, "list", /* @__PURE__ */ new Map());
    this.components.add(Bs.uuid, this);
  }
  /**
   * Creates a new {@link MiniMap} instance associated with the given world.
   * If a {@link MiniMap} instance already exists for the given world, an error will be thrown.
   *
   * @param world - The {@link World} for which to create a {@link MiniMap} instance.
   * @returns The newly created {@link MiniMap} instance.
   * @throws Will throw an error if a {@link MiniMap} instance already exists for the given world.
   */
  create(t) {
    if (this.list.has(t.uuid))
      throw new Error("This world already has a minimap!");
    const e = new nh(t, this.components);
    return this.list.set(t.uuid, e), e;
  }
  /**
   * Deletes a {@link MiniMap} instance associated with the given world ID.
   * If a {@link MiniMap} instance does not exist for the given ID, nothing happens.
   *
   * @param id - The unique identifier of the world for which to delete the {@link MiniMap} instance.
   * @returns {void}
   */
  delete(t) {
    const e = this.list.get(t);
    e && e.dispose(), this.list.delete(t);
  }
  /** {@link Disposable.dispose} */
  dispose() {
    for (const [t, e] of this.list)
      e.dispose();
    this.list.clear(), this.onDisposed.trigger();
  }
  /** {@link Updateable.update} */
  update() {
    for (const [t, e] of this.list)
      e.update();
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(Bs, "uuid", "39ad6aad-84c8-4adf-a1e0-7f25313a9e7f");
let Xr = Bs;
const Vs = class Vs extends At {
  constructor(t) {
    super(t);
    /** {@link Component.enabled} */
    C(this, "enabled", !0);
    t.add(Vs.uuid, this);
  }
  /**
   * Utility method to calculate the distance from a point to a line segment.
   *
   * @param point - The point from which to calculate the distance.
   * @param lineStart - The start point of the line segment.
   * @param lineEnd - The end point of the line segment.
   * @param clamp - If true, the distance will be clamped to the line segment's length.
   * @returns The distance from the point to the line segment.
   */
  static distanceFromPointToLine(t, e, s, n = !1) {
    const r = new D.Line3(), o = new D.Vector3();
    return r.set(e, s), r.closestPointToPoint(t, n, o), o.distanceTo(t);
  }
  /**
   * Method to get the face of a mesh that contains a given triangle index.
   * It also returns the edges of the found face and their indices.
   *
   * @param mesh - The mesh to get the face from. It must be indexed.
   * @param triangleIndex - The index of the triangle within the mesh.
   * @param instance - The instance of the mesh (optional).
   * @returns An object containing the edges of the found face and their indices, or null if no face was found.
   */
  getFace(t, e, s) {
    if (!t.geometry.index)
      throw new Error("Geometry must be indexed!");
    const n = /* @__PURE__ */ new Map(), r = t.geometry.index.array, { plane: o } = this.getFaceData(
      e,
      s,
      t
    ), a = [];
    for (let I = 0; I < r.length / 3; I++) {
      const { plane: u, edges: d } = this.getFaceData(I, s, t);
      if (u.equals(o)) {
        a.push({ index: I, edges: d });
        for (const { id: E, points: T, distance: p } of d)
          n.set(E, { points: T, distance: p });
      }
    }
    let l = 0;
    const h = /* @__PURE__ */ new Map(), f = /* @__PURE__ */ new Map();
    for (const { index: I, edges: u } of a) {
      const d = /* @__PURE__ */ new Map();
      for (const { id: F } of u)
        if (h.has(F)) {
          const O = h.get(F);
          d.set(F, O);
        }
      const E = u.map((F) => F.id);
      if (!d.size) {
        const F = l++;
        for (const { id: O } of u)
          h.set(O, F);
        f.set(F, {
          edges: new Set(E),
          indices: /* @__PURE__ */ new Set([I])
        });
        continue;
      }
      let T = null;
      const p = /* @__PURE__ */ new Set(), R = new Set(E);
      for (const [F, O] of d) {
        T === null ? T = O : O !== T && p.add(O), h.delete(F);
        const { edges: y } = f.get(O);
        y.delete(F), R.delete(F);
      }
      if (T === null)
        throw new Error("Error computing face!");
      const S = f.get(T), { indices: m } = S;
      m.add(I);
      for (const F of R) {
        h.set(F, T);
        const { edges: O } = S;
        O.add(F);
      }
      for (const F of p) {
        const O = f.get(F), { edges: y, indices: w } = O, L = f.get(T), { edges: b, indices: Y } = L;
        for (const N of y)
          b.add(N), h.set(N, T);
        for (const N of w)
          Y.add(N);
        f.delete(F);
      }
    }
    for (const [I, { indices: u, edges: d }] of f)
      if (u.has(e)) {
        const E = [];
        for (const T of d) {
          const p = n.get(T);
          E.push(p);
        }
        return { edges: E, indices: u };
      }
    return null;
  }
  /**
   * Method to get the vertices and normal of a mesh face at a given index.
   * It also applies instance transformation if provided.
   *
   * @param mesh - The mesh to get the face from. It must be indexed.
   * @param faceIndex - The index of the face within the mesh.
   * @param instance - The instance of the mesh (optional).
   * @returns An object containing the vertices and normal of the face.
   * @throws Will throw an error if the geometry is not indexed.
   */
  getVerticesAndNormal(t, e, s) {
    if (!t.geometry.index)
      throw new Error("Geometry must be indexed!");
    const n = t.geometry.index.array, r = t.geometry.attributes.position.array, o = t.geometry.attributes.normal.array, a = n[e * 3] * 3, l = n[e * 3 + 1] * 3, h = n[e * 3 + 2] * 3, f = new D.Vector3(r[a], r[a + 1], r[a + 2]), I = new D.Vector3(r[l], r[l + 1], r[l + 2]), u = new D.Vector3(r[h], r[h + 1], r[h + 2]), d = new D.Vector3(o[a], o[a + 1], o[a + 2]), E = new D.Vector3(o[l], o[l + 1], o[l + 2]), T = new D.Vector3(o[h], o[h + 1], o[h + 2]), p = (d.x + E.x + T.x) / 3, R = (d.y + E.y + T.y) / 3, S = (d.z + E.z + T.z) / 3, m = new D.Vector3(p, R, S);
    if (s !== void 0 && t instanceof D.InstancedMesh) {
      const F = new D.Matrix4();
      t.getMatrixAt(s, F);
      const O = new D.Matrix4();
      O.extractRotation(F), m.applyMatrix4(O), f.applyMatrix4(F), I.applyMatrix4(F), u.applyMatrix4(F);
    }
    return { p1: f, p2: I, p3: u, faceNormal: m };
  }
  /**
   * Method to round the vector's components to a specified number of decimal places.
   * This is used to ensure numerical precision in edge detection.
   *
   * @param vector - The vector to round.
   * @returns The vector with rounded components.
   */
  round(t) {
    t.x = Math.trunc(t.x * 1e3) / 1e3, t.y = Math.trunc(t.y * 1e3) / 1e3, t.z = Math.trunc(t.z * 1e3) / 1e3;
  }
  /**
   * Calculates the volume of a set of fragments.
   *
   * @param frags - A map of fragment IDs to their corresponding item IDs.
   * @returns The total volume of the fragments and the bounding sphere.
   *
   * @remarks
   * This method creates a set of instanced meshes from the given fragments and item IDs.
   * It then calculates the volume of each mesh and returns the total volume and its bounding sphere.
   *
   * @throws Will throw an error if the geometry of the meshes is not indexed.
   * @throws Will throw an error if the fragment manager is not available.
   */
  getVolumeFromFragments(t) {
    const e = this.components.get(Tt), s = new D.Matrix4(), n = [];
    for (const o in t) {
      const a = e.list.get(o);
      if (!a)
        continue;
      const l = t[o];
      let h = 0;
      for (const u of l) {
        const d = a.getInstancesIDs(u);
        d && (h += d.size);
      }
      const f = new D.InstancedMesh(
        a.mesh.geometry,
        void 0,
        h
      );
      let I = 0;
      for (const u of l) {
        const d = a.getInstancesIDs(u);
        if (d)
          for (const E of d)
            a.mesh.getMatrixAt(E, s), f.setMatrixAt(I++, s);
      }
      n.push(f);
    }
    const r = this.getVolumeFromMeshes(n);
    for (const o of n)
      o.geometry = null, o.material = [], o.dispose();
    return r;
  }
  /**
   * Calculates the total volume of a set of meshes.
   *
   * @param meshes - An array of meshes or instanced meshes to calculate the volume from.
   * @returns The total volume of the meshes and the bounding sphere.
   *
   * @remarks
   * This method calculates the volume of each mesh in the provided array and returns the total volume
   * and its bounding sphere.
   *
   */
  getVolumeFromMeshes(t) {
    let e = 0;
    for (const s of t)
      e += this.getVolumeOfMesh(s);
    return e;
  }
  getFaceData(t, e, s) {
    const n = this.getVerticesAndNormal(s, t, e), { p1: r, p2: o, p3: a, faceNormal: l } = n;
    this.round(r), this.round(o), this.round(a), this.round(l);
    const h = [
      { id: `${r.x}|${r.y}|${r.z}`, value: r },
      { id: `${o.x}|${o.y}|${o.z}`, value: o },
      { id: `${a.x}|${a.y}|${a.z}`, value: a }
    ];
    h.sort((S, m) => S.id < m.id ? -1 : S.id > m.id ? 1 : 0);
    const [
      { id: f, value: I },
      { id: u, value: d },
      { id: E, value: T }
    ] = h, p = [
      {
        id: `${f}|${u}`,
        distance: I.distanceTo(d),
        points: [I, d]
      },
      {
        id: `${u}|${E}`,
        distance: d.distanceTo(T),
        points: [d, T]
      },
      {
        id: `${f}|${E}`,
        distance: I.distanceTo(T),
        points: [I, T]
      }
    ], R = new D.Plane();
    return R.setFromNormalAndCoplanarPoint(l, r), R.constant = Math.round(R.constant * 10) / 10, { plane: R, edges: p };
  }
  // https://stackoverflow.com/a/1568551
  getVolumeOfMesh(t) {
    let e = 0;
    const s = new D.Vector3(), n = new D.Vector3(), r = new D.Vector3(), { index: o } = t.geometry, a = t.geometry.attributes.position.array;
    if (!o)
      return console.warn("Geometry must be indexed to compute its volume!"), 0;
    const l = [];
    if (t instanceof D.InstancedMesh)
      for (let f = 0; f < t.count; f++) {
        const I = new D.Matrix4();
        t.getMatrixAt(f, I), l.push(I);
      }
    else
      l.push(new D.Matrix4().identity());
    const { matrixWorld: h } = t;
    for (let f = 0; f < o.array.length - 2; f += 3)
      for (const I of l) {
        const u = I.multiply(h), d = o.array[f] * 3, E = o.array[f + 1] * 3, T = o.array[f + 2] * 3;
        s.set(a[d], a[d + 1], a[d + 2]).applyMatrix4(u), n.set(a[E], a[E + 1], a[E + 2]).applyMatrix4(u), r.set(a[T], a[T + 1], a[T + 2]).applyMatrix4(u), e += this.getSignedVolumeOfTriangle(s, n, r);
      }
    return Math.abs(e);
  }
  getSignedVolumeOfTriangle(t, e, s) {
    const n = s.x * e.y * t.z, r = e.x * s.y * t.z, o = s.x * t.y * e.z, a = t.x * s.y * e.z, l = e.x * t.y * s.z, h = t.x * e.y * s.z;
    return 1 / 6 * (-n + r + o - a - l + h);
  }
};
/**
 * A unique identifier for the component.
 * This UUID is used to register the component within the Components system.
 */
C(Vs, "uuid", "267ca032-672f-4cb0-afa9-d24e904f39d6");
let $r = Vs;
class ks {
  constructor(i) {
    // Used when the facet is a requirement
    // On IDSEntity is always required
    C(this, "cardinality", "required");
    // When using this facet as a requirement, instructions can be given for the authors of the IFC.
    C(this, "instructions");
    C(this, "evalRequirement", (i, t, e, s) => {
      const n = {
        parameter: e,
        currentValue: i,
        requiredValue: t.parameter,
        pass: !1
      };
      s && this.addCheckResult(n, s);
      let r = !1;
      if (t.type === "simple" && (r = i === t.parameter), t.type === "enumeration" && (r = t.parameter.includes(i)), t.type === "pattern" && (r = new RegExp(t.parameter).test(String(i))), t.type === "length") {
        const { min: o, length: a, max: l } = t.parameter;
        a !== void 0 && (r = String(i).length === a), o !== void 0 && (r = String(i).length >= o), l !== void 0 && (r = String(i).length <= l);
      }
      if (t.type === "bounds" && typeof i == "number") {
        const { min: o, minInclusive: a, max: l, maxInclusive: h } = t.parameter;
        let f = !0, I = !0;
        o !== void 0 && (f = a ? i <= o : i < o), l !== void 0 && (I = h ? i >= l : i > l), r = f && I;
      }
      return this.cardinality === "prohibited" && (r = !r), this.cardinality === "optional" && (r = !0), n.pass = r, n.pass;
    });
    C(this, "testResult", []);
    this.components = i;
  }
  addCheckResult(i, t) {
    const e = t.findIndex(
      ({ parameter: s }) => s === i.parameter
    );
    e !== -1 ? t[e] = i : t.push(i);
  }
  saveResult(i, t) {
    const { GlobalId: e } = i;
    if (!e)
      return;
    const { value: s } = e, n = {
      expressID: s,
      pass: t,
      checks: [],
      cardinality: this.cardinality
    };
    this.testResult.push(n);
  }
}
const Re = (c, i) => {
  let t = "";
  return i ? (i.type === "simple" && (t = `<ids:simpleValue>${i.parameter}</ids:simpleValue>`), i.type === "enumeration" && (t = `<xs:restriction base="xs:string">
    ${i.parameter.map((n) => `<xs:enumeration value="${n}" />`).join(`\r
`)}
    </xs:restriction>`), i.type === "pattern" && (t = `<xs:restriction base="xs:string">
      <xs:pattern value="${i.parameter}" />
    </xs:restriction>`), `<ids:${c[0].toLowerCase() + c.slice(1)}>
    ${t}
  </ids:${c[0].toLowerCase() + c.slice(1)}>`) : t;
};
class rh extends ks {
  constructor(t, e) {
    super(t);
    C(this, "facetType", "Attribute");
    C(this, "name");
    C(this, "value");
    this.name = e;
  }
  serialize(t) {
    const e = Re("Name", this.name), s = Re("Value", this.value);
    let n = "";
    return t === "requirement" && (n += `cardinality="${this.cardinality}"`, n += this.instructions ? `instructions="${this.instructions}"` : ""), `<ids:attribute ${n}>
  ${e}
  ${s}
</ids:attribute>`;
  }
  // This can be very ineficcient as we do not have an easy way to get an entity based on an attribute
  // Right now, all entities must be iterated.
  // When the new IfcEntitiesFinder comes, this can become easier.
  // This may be greatly increase in performance if the applicability has any of the other facets and this is applied the latest
  async getEntities() {
    return [];
  }
  // async getEntities(
  //   model: FRAGS.FragmentsGroup,
  //   collector: FRAGS.IfcProperties = {},
  // ) {
  //   return [];
  //   // for (const expressID in model) {
  //   //   if (collector[expressID]) continue;
  //   //   const entity = model[expressID];
  //   //   // Check if the attribute exists
  //   //   const attribute = entity[this.name];
  //   //   const attributeExists = !!attribute;
  //   //   // Check if the attribute value matches
  //   //   let valueMatches = true;
  //   //   if (attributeExists && this.value && this.value.value) {
  //   //     if (this.value.type === "simpleValue") {
  //   //       valueMatches = attribute.value === this.value.value;
  //   //     }
  //   //     if (this.value.type === "restriction") {
  //   //       const regex = new RegExp(this.value.value);
  //   //       valueMatches = regex.test(attribute.value);
  //   //     }
  //   //   }
  //   //   if (attributeExists && valueMatches) {
  //   //     collector[entity.expressID] = entity;
  //   //   }
  //   // }
  // }
  // https://github.com/buildingSMART/IDS/tree/development/Documentation/ImplementersDocumentation/TestCases/attribute
  // Test cases from buildingSMART repo have been tested and they all match with the expected result
  // All invalid cases have been treated as failures
  // FragmentsGroup do not hold some of the entities used in the tests
  async test(t) {
    var s;
    this.testResult = [];
    for (const n in t) {
      const r = Number(n), o = t[r], a = [], l = {
        guid: (s = o.GlobalId) == null ? void 0 : s.value,
        expressID: r,
        pass: !1,
        checks: a,
        cardinality: this.cardinality
      };
      this.testResult.push(l);
      const f = Object.keys(o).filter((u) => {
        const d = this.evalRequirement(u, this.name, "Name"), E = o[u];
        return d && E === null ? this.cardinality === "optional" || this.cardinality === "prohibited" : d && (E == null ? void 0 : E.type) === 3 && E.value === 2 || d && Array.isArray(E) && E.length === 0 || d && (E == null ? void 0 : E.type) === 1 && E.value.trim() === "" ? !1 : d;
      }), I = f.length > 0;
      if (a.push({
        parameter: "Name",
        currentValue: I ? f[0] : null,
        requiredValue: this.name.parameter,
        pass: this.cardinality === "prohibited" ? !I : I
      }), this.value)
        if (f[0]) {
          const u = o[f[0]];
          (u == null ? void 0 : u.type) === 5 ? a.push({
            parameter: "Value",
            currentValue: null,
            requiredValue: this.value.parameter,
            pass: this.cardinality === "prohibited"
          }) : this.evalRequirement(
            u ? u.value : null,
            this.value,
            "Value",
            a
          );
        } else
          a.push({
            parameter: "Value",
            currentValue: null,
            requiredValue: this.value.parameter,
            pass: this.cardinality === "prohibited"
          });
      l.pass = a.every(({ pass: u }) => u);
    }
    const e = [...this.testResult];
    return this.testResult = [], e;
  }
}
class oh extends ks {
  constructor(t, e) {
    super(t);
    C(this, "facetType", "Classification");
    C(this, "system");
    C(this, "value");
    C(this, "uri");
    this.system = e;
  }
  serialize(t) {
    const e = Re("System", this.system), s = Re("Value", this.value);
    let n = "";
    return t === "requirement" && (n += `cardinality="${this.cardinality}"`, n += this.uri ? `uri=${this.uri}` : "", n += this.instructions ? `instructions="${this.instructions}"` : ""), `<ids:classification ${n}>
  ${e}
  ${s}
</ids:classification>`;
  }
  async getEntities(t, e = {}) {
    var h;
    const s = [], n = await t.getAllPropertiesOfType(
      k.IFCCLASSIFICATIONREFERENCE
    ), r = await t.getAllPropertiesOfType(
      k.IFCCLASSIFICATION
    ), o = { ...n, ...r }, a = [];
    for (const f in o) {
      const I = Number(f), u = await t.getProperties(I);
      if (!u)
        continue;
      const d = (h = u.ReferencedSource) == null ? void 0 : h.value;
      if (!d)
        continue;
      const E = await t.getProperties(d);
      !E || !this.evalSystem(E) || !this.evalValue(u) || !this.evalURI(u) || a.push(I);
    }
    const l = this.components.get(Gt);
    for (const f of a) {
      const I = l.getEntitiesWithRelation(
        t,
        "HasAssociations",
        f
      );
      for (const u of I) {
        if (u in e)
          continue;
        const d = await t.getProperties(u);
        d && (e[u] = d, s.push(u));
      }
    }
    return s;
  }
  async test(t, e) {
    var n;
    this.testResult = [];
    for (const r in t) {
      const o = Number(r), a = t[o], l = [], h = {
        guid: (n = a.GlobalId) == null ? void 0 : n.value,
        expressID: o,
        pass: !1,
        checks: l,
        cardinality: this.cardinality
      };
      this.testResult.push(h);
      let f = !0;
      const I = await this.getSystems(e, o), u = I.map((d) => this.getSystemName(d)).filter((d) => d);
      for (const d of I) {
        if (!this.evalSystem(d, l))
          continue;
        if (f = !1, !(this.value && this.system))
          break;
        if (d.type !== k.IFCCLASSIFICATIONREFERENCE)
          continue;
        const T = !this.value || this.evalValue(d, l), p = !this.uri || this.evalURI(d, l);
        if (T && p)
          break;
      }
      f && this.addCheckResult(
        {
          parameter: "System",
          currentValue: u,
          requiredValue: this.system,
          pass: this.cardinality === "optional"
        },
        l
      ), h.pass = l.every(({ pass: d }) => d);
    }
    const s = [...this.testResult];
    return this.testResult = [], s;
  }
  async processReferencedSource(t, e) {
    var r;
    const s = (r = e.ReferencedSource) == null ? void 0 : r.value;
    if (!s)
      return null;
    const n = await t.getProperties(s);
    return n ? (n.type === k.IFCCLASSIFICATIONREFERENCE && (n.ReferencedSource = await this.processReferencedSource(
      t,
      n
    )), n) : null;
  }
  async getSystems(t, e) {
    var f;
    const s = [], n = this.components.get(Gt), r = n.getEntityRelations(
      t,
      e,
      "HasAssociations"
    );
    if (r)
      for (const I of r) {
        const u = await t.getProperties(I);
        u && (u.type === k.IFCCLASSIFICATION && s.push(u), u.type === k.IFCCLASSIFICATIONREFERENCE && (u.ReferencedSource = await this.processReferencedSource(
          t,
          u
        ), u.ReferencedSource && s.push(u)));
      }
    const o = s.map((I) => {
      var u, d, E;
      return I.type === k.IFCCLASSIFICATION ? (u = I.Name) == null ? void 0 : u.value : I.type === k.IFCCLASSIFICATIONREFERENCE ? (E = (d = I.ReferencedSource) == null ? void 0 : d.Name) == null ? void 0 : E.value : null;
    }).filter((I) => I), a = n.getEntityRelations(t, e, "IsTypedBy");
    if (!(a && a[0]))
      return s;
    const l = a[0], h = n.getEntityRelations(
      t,
      l,
      "HasAssociations"
    );
    if (h)
      for (const I of h) {
        const u = await t.getProperties(I);
        if (u) {
          if (u.type === k.IFCCLASSIFICATION) {
            if (o.includes((f = u.Name) == null ? void 0 : f.value))
              continue;
            s.push(u);
          }
          u.type === k.IFCCLASSIFICATIONREFERENCE && (u.ReferencedSource = await this.processReferencedSource(
            t,
            u
          ), u.ReferencedSource && s.push(u));
        }
      }
    return s;
  }
  getSystemName(t) {
    var e, s, n, r;
    if (t.type === k.IFCCLASSIFICATION)
      return (e = t.Name) == null ? void 0 : e.value;
    if (t.type === k.IFCCLASSIFICATIONREFERENCE) {
      if (((s = t.ReferencedSource) == null ? void 0 : s.type) === k.IFCCLASSIFICATIONREFERENCE)
        return this.getSystemName(t.ReferencedSource);
      if (((n = t.ReferencedSource) == null ? void 0 : n.type) === k.IFCCLASSIFICATION)
        return (r = t.ReferencedSource.Name) == null ? void 0 : r.value;
    }
    return null;
  }
  getAllReferenceIdentifications(t) {
    if (t.type !== k.IFCCLASSIFICATIONREFERENCE)
      return null;
    const e = [];
    if (t.Identification && e.push(t.Identification.value), t.ReferencedSource) {
      const s = this.getAllReferenceIdentifications(
        t.ReferencedSource
      );
      s && e.push(...s);
    }
    return e;
  }
  evalSystem(t, e) {
    const s = this.getSystemName(t);
    return this.evalRequirement(s, this.system, "System", e);
  }
  evalValue(t, e) {
    if (!this.value)
      return !0;
    const s = this.getAllReferenceIdentifications(t);
    if (!s)
      return !1;
    const n = s.find((r) => this.value ? this.evalRequirement(r, this.value, "Value") : !1);
    return e && this.addCheckResult(
      {
        parameter: "Value",
        currentValue: n ?? null,
        requiredValue: this.value,
        pass: !!n
      },
      e
    ), !!n;
  }
  evalURI(t, e) {
    var n;
    return this.uri ? this.evalRequirement(
      (n = t.Location) == null ? void 0 : n.value,
      {
        type: "simple",
        parameter: this.uri
      },
      "URI",
      e
    ) : !0;
  }
}
class ah extends ks {
  constructor(t, e) {
    super(t);
    C(this, "facetType", "Entity");
    C(this, "name");
    C(this, "predefinedType");
    this.name = e;
  }
  serialize(t) {
    const e = Re("Name", this.name), s = Re("Name", this.predefinedType);
    let n = "";
    return t === "requirement" && (n += `cardinality="${this.cardinality}"`, n += this.instructions ? `instructions="${this.instructions}"` : ""), `<ids:entity ${n}>
  ${e}
  ${s}
</ids:entity>`;
  }
  // IFCSURFACESTYLEREFRACTION is not present in the FragmentsGroup
  // IFCSURFACESTYLERENDERING is not present in the FragmentsGroup
  async getEntities(t, e = {}) {
    const s = Object.entries(ms), n = [];
    for (const [a] of s)
      await this.evalName({ type: a }) && n.push(Number(a));
    let r = {};
    for (const a of n) {
      const l = await t.getAllPropertiesOfType(a);
      l && (r = { ...r, ...l });
    }
    if (!this.predefinedType) {
      for (const a in r)
        a in e || (e[a] = r[a]);
      return Object.keys(r).map(Number);
    }
    const o = [];
    for (const a in r) {
      const l = Number(a);
      if (l in e)
        continue;
      const h = r[l];
      await this.evalPredefinedType(t, h) && (e[l] = h, o.push(l));
    }
    return o;
  }
  async test(t, e) {
    var s;
    this.testResult = [];
    for (const n in t) {
      const r = Number(n), o = t[r], a = [], l = {
        guid: (s = o.GlobalId) == null ? void 0 : s.value,
        expressID: r,
        pass: !1,
        checks: a,
        cardinality: this.cardinality
      };
      this.testResult.push(l), await this.evalName(o, a), await this.evalPredefinedType(e, o, a), l.pass = a.every(({ pass: h }) => h);
    }
    return this.testResult;
  }
  async evalName(t, e) {
    const s = ms[t.type];
    return this.evalRequirement(s, this.name, "Name", e);
  }
  async evalPredefinedType(t, e, s) {
    var l, h, f, I;
    if (!this.predefinedType)
      return null;
    const n = this.components.get(Gt), r = typeof this.predefinedType.parameter == "string" && this.predefinedType.parameter === "USERDEFINED";
    let o = (l = e.PredefinedType) == null ? void 0 : l.value;
    if (o === "USERDEFINED" && !r) {
      const d = Object.keys(e).find(
        (E) => /^((?!Predefined).)*Type$/.test(E)
      );
      o = d ? (h = e[d]) == null ? void 0 : h.value : "USERDEFINED";
    }
    if (!o) {
      const u = n.getEntityRelations(
        t,
        e.expressID,
        "IsTypedBy"
      );
      if (u && u[0]) {
        const d = await t.getProperties(u[0]);
        if (d && (o = (f = d.PredefinedType) == null ? void 0 : f.value, o === "USERDEFINED" && !r)) {
          const T = Object.keys(d).find(
            (p) => /^((?!Predefined).)*Type$/.test(p)
          );
          o = T ? (I = d[T]) == null ? void 0 : I.value : "USERDEFINED";
        }
      }
    }
    return this.evalRequirement(
      o,
      this.predefinedType,
      "PredefinedType",
      s
    );
  }
}
class ch extends ks {
  constructor(t, e, s) {
    super(t);
    C(this, "facetType", "Property");
    C(this, "propertySet");
    C(this, "baseName");
    C(this, "value");
    C(this, "dataType");
    C(this, "uri");
    // These are defined by the IDS specification
    C(this, "_unsupportedTypes", [
      k.IFCCOMPLEXPROPERTY,
      k.IFCPHYSICALCOMPLEXQUANTITY
    ]);
    this.propertySet = e, this.baseName = s;
  }
  serialize(t) {
    const e = Re("PropertySet", this.propertySet), s = Re("BaseName", this.baseName), n = Re("Value", this.value), r = this.dataType ? `dataType=${this.dataType}` : "";
    let o = "";
    return t === "requirement" && (o += `cardinality="${this.cardinality}"`, o += this.uri ? `uri=${this.uri}` : "", o += this.instructions ? `instructions="${this.instructions}"` : ""), `<ids:property ${r} ${o}>
  ${e}
  ${s}
  ${n}
</ids:property>`;
  }
  async getEntities(t, e = {}) {
    var l, h;
    let s = {};
    const n = await t.getAllPropertiesOfType(k.IFCPROPERTYSET);
    s = { ...s, ...n };
    const r = await t.getAllPropertiesOfType(k.IFCELEMENTQUANTITY);
    if (s = { ...s, ...r }, Object.keys(s).length === 0)
      return [];
    const o = [];
    for (const f in s) {
      const I = Number(f), u = await t.getProperties(I);
      if (!u || !(((l = u.Name) == null ? void 0 : l.value) === this.propertySet.parameter))
        continue;
      let E;
      if (u.type === k.IFCPROPERTYSET && (E = "HasProperties"), u.type === k.IFCELEMENTQUANTITY && (E = "Quantities"), !!E)
        for (const T of u[E]) {
          const p = await t.getProperties(T.value);
          if (!(!p || !(((h = p.Name) == null ? void 0 : h.value) === this.baseName.parameter))) {
            if (this.value) {
              const S = Object.keys(p).find(
                (F) => F.endsWith("Value")
              );
              if (!S || !(p[S].value === this.value.parameter))
                continue;
            }
            o.push(I);
          }
        }
    }
    const a = this.components.get(Gt);
    for (const f of o) {
      const I = a.getEntitiesWithRelation(
        t,
        "IsDefinedBy",
        f
      );
      for (const u of I) {
        if (u in e)
          continue;
        const d = await t.getProperties(u);
        d && (e[u] = d);
      }
    }
    return [];
  }
  async test(t, e) {
    var n;
    this.testResult = [];
    for (const r in t) {
      const o = Number(r), a = t[o], l = [], h = {
        guid: (n = a.GlobalId) == null ? void 0 : n.value,
        expressID: o,
        pass: !1,
        checks: l,
        cardinality: this.cardinality
      };
      this.testResult.push(h);
      const I = (await this.getPsets(e, o)).filter((u) => {
        var E;
        return this.evalRequirement(
          ((E = u.Name) == null ? void 0 : E.value) ?? null,
          this.propertySet,
          "PropertySet"
        ) ? (l.push({
          currentValue: u.Name.value,
          parameter: "PropertySet",
          pass: !0,
          requiredValue: this.propertySet.parameter
        }), !0) : !1;
      });
      if (I.length === 0) {
        l.push({
          currentValue: null,
          parameter: "PropertySet",
          pass: !1,
          requiredValue: this.propertySet.parameter
        });
        continue;
      }
      for (const u of I) {
        const d = this.getItemsAttrName(u.type);
        if (!d) {
          l.push({
            currentValue: null,
            parameter: "BaseName",
            pass: !1,
            requiredValue: this.baseName.parameter
          });
          continue;
        }
        const T = u[d].filter((p) => {
          var S;
          return this._unsupportedTypes.includes(p.type) || !this.evalRequirement(
            ((S = p.Name) == null ? void 0 : S.value) ?? null,
            this.baseName,
            "BaseName"
          ) ? !1 : (l.push({
            currentValue: p.Name.value,
            parameter: "BaseName",
            pass: !0,
            requiredValue: this.baseName.parameter
          }), !0);
        });
        if (T.length === 0) {
          l.push({
            currentValue: null,
            parameter: "BaseName",
            pass: !1,
            requiredValue: this.baseName.parameter
          });
          continue;
        }
        for (const p of T)
          this.evalValue(p, l), this.evalDataType(p, l), this.evalURI();
      }
      h.pass = l.every(({ pass: u }) => u);
    }
    const s = [...this.testResult];
    return this.testResult = [], s;
  }
  getItemsAttrName(t) {
    let e;
    return t === k.IFCPROPERTYSET && (e = "HasProperties"), t === k.IFCELEMENTQUANTITY && (e = "Quantities"), e;
  }
  getValueKey(t) {
    return Object.keys(t).find(
      (e) => e.endsWith("Value") || e.endsWith("Values")
    );
  }
  async getPsetProps(t, e, s) {
    const n = structuredClone(e), r = [], o = n[s];
    if (!o)
      return r;
    for (const { value: a } of o) {
      const l = await t.getProperties(a);
      l && r.push(l);
    }
    return n[s] = r, n;
  }
  async getTypePsets(t, e) {
    const s = [], r = this.components.get(Gt).getEntityRelations(t, e, "IsTypedBy");
    if (!(r && r[0]))
      return s;
    const o = await t.getProperties(r[0]);
    if (!(o && "HasPropertySets" in o && Array.isArray(o.HasPropertySets)))
      return s;
    for (const { value: a } of o.HasPropertySets) {
      const l = await t.getProperties(a);
      if (!(l && "HasProperties" in l && Array.isArray(l.HasProperties)))
        continue;
      const h = await this.getPsetProps(t, l, "HasProperties");
      s.push(h);
    }
    return s;
  }
  async getPsets(t, e) {
    const s = await this.getTypePsets(t, e), r = this.components.get(Gt).getEntityRelations(
      t,
      e,
      "IsDefinedBy"
    );
    if (!r)
      return s;
    for (const o of r) {
      const a = await t.getProperties(o);
      if (!a)
        continue;
      const l = this.getItemsAttrName(a.type);
      if (!l)
        continue;
      const h = await this.getPsetProps(t, a, l);
      s.push(h);
    }
    return s;
  }
  // IFCPROPERTYBOUNDEDVALUE are not supported yet
  // IFCPROPERTYTABLEVALUE are not supported yet
  // Work must to be done to convert numerical value units to IDS-nominated standard units https://github.com/buildingSMART/IDS/blob/development/Documentation/UserManual/units.md
  evalValue(t, e) {
    const s = this.getValueKey(t), n = t[s];
    if (this.value) {
      if (!n)
        return e == null || e.push({
          parameter: "Value",
          currentValue: null,
          pass: !1,
          requiredValue: this.value.parameter
        }), !1;
      const r = structuredClone(this.value);
      if (n.name === "IFCLABEL" && r.type === "simple" && (r.parameter = String(r.parameter)), (t.type === k.IFCPROPERTYLISTVALUE || t.type === k.IFCPROPERTYENUMERATEDVALUE) && Array.isArray(n)) {
        const a = n.map((h) => h.value), l = n.find((h) => r ? this.evalRequirement(h.value, r, "Value") : !1);
        return e == null || e.push({
          currentValue: a,
          pass: !!l,
          parameter: "Value",
          requiredValue: r.parameter
        }), !!l;
      }
      return this.evalRequirement(
        n.value,
        r,
        "Value",
        e
      );
    }
    return s ? n.type === 3 && n.value === 2 ? (e == null || e.push({
      parameter: "Value",
      currentValue: null,
      pass: !1,
      requiredValue: null
    }), !1) : n.type === 1 && n.value.trim() === "" ? (e == null || e.push({
      parameter: "Value",
      currentValue: "",
      pass: !1,
      requiredValue: null
    }), !1) : !0 : !0;
  }
  evalDataType(t, e) {
    if (!this.dataType)
      return !0;
    const s = this.getValueKey(t), n = t[s];
    if (!n)
      return e == null || e.push({
        parameter: "DataType",
        currentValue: null,
        pass: !1,
        requiredValue: this.dataType
      }), !1;
    if ((t.type === k.IFCPROPERTYLISTVALUE || t.type === k.IFCPROPERTYENUMERATEDVALUE) && Array.isArray(n) && n[0]) {
      const o = n[0].name;
      return this.evalRequirement(
        o,
        {
          type: "simple",
          parameter: this.dataType
        },
        "DataType",
        e
      );
    }
    return this.evalRequirement(
      n.name,
      {
        type: "simple",
        parameter: this.dataType
      },
      "DataType",
      e
    );
  }
  evalURI() {
    return !0;
  }
}
class lh {
  constructor(i, t, e) {
    C(this, "name");
    C(this, "ifcVersion", /* @__PURE__ */ new Set());
    C(this, "identifier", oe.create());
    C(this, "description");
    C(this, "instructions");
    C(this, "requirementsDescription");
    C(this, "applicability", new we());
    C(this, "requirements", new we());
    C(this, "components");
    this.components = i, this.name = t;
    for (const s of e)
      this.ifcVersion.add(s);
  }
  set(i) {
    const t = i, e = this;
    for (const n in i) {
      if (n === "identifier")
        continue;
      const r = t[n];
      n in this && (e[n] = r);
    }
    return this.components.get(bn).list.set(this.identifier, this), this;
  }
  /**
   * Tests the model to test against the specification's requirements.
   *
   * @param model - The model to be tested.
   * @returns An array representing the test results.
   * If no requirements are defined for the specification, an empty array is returned.
   */
  async test(i) {
    let t = [];
    if (this.requirements.size === 0)
      return t;
    const e = {};
    for (const n of this.applicability)
      await n.getEntities(i, e);
    return t = await [...this.requirements][0].test(e, i), t;
  }
  /**
   * Serializes the IDSSpecification instance into XML format.
   *
   * @remarks This method is not meant to be used directly. It is used by the IDSSpecifications component.
   *
   * @returns The XML representation of the IDSSpecification.
   */
  serialize() {
    const i = `name="${this.name}"`, t = this.identifier ? `identifier="${this.identifier}"` : "", e = this.description ? `description="${this.description}"` : "", s = this.instructions ? `instructions="${this.instructions}"` : "";
    return `<ids:specification ifcVersion="${[...this.ifcVersion].join(" ")}" ${i} ${t} ${e} ${s}>
      <ids:applicability minOccurs="1" maxOccurs="unbounded">
        ${[...this.applicability].map((r) => r.serialize("applicability"))}
      </ids:applicability>
      <ids:requirements>
        ${[...this.requirements].map((r) => r.serialize("requirement"))}
      </ids:requirements>
    </ids:specification>`;
  }
}
const ge = (c) => {
  if (!c)
    return;
  const i = {};
  if ("simpleValue" in c && (i.type = "simple", i.parameter = c.simpleValue), "restriction" in c) {
    const t = c.restriction;
    if ("pattern" in t && (i.type = "pattern", i.parameter = t.pattern.value), "enumeration" in t) {
      i.type = "enumeration";
      const e = t.enumeration.map(
        ({ value: s }) => s
      );
      i.parameter = e;
    }
  }
  if (i.parameter !== void 0)
    return i;
}, Zr = (c, i) => {
  const t = [];
  for (const e of i) {
    const s = e.name, n = ge(s);
    if (!n)
      continue;
    const r = new ah(c, n);
    e.cardinality && (r.cardinality = e.cardinality), r.predefinedType = ge(e.predefinedType), r.instructions = e.instructions, t.push(r);
  }
  return t;
}, hh = (c, i) => {
  const t = [];
  for (const e of i) {
    const s = e.name, n = ge(s);
    if (!n)
      continue;
    const r = new rh(c, n);
    e.cardinality && (r.cardinality = e.cardinality), r.value = ge(e.value), r.instructions = e.instructions, t.push(r);
  }
  return t;
}, uh = (c, i) => {
  const t = [];
  for (const e of i) {
    const s = e.system, n = ge(s);
    if (!n)
      continue;
    const r = new oh(c, n);
    e.cardinality && (r.cardinality = e.cardinality);
    const o = ge(e.value);
    (o == null ? void 0 : o.type) === "simple" && (o.parameter = String(o.parameter)), (o == null ? void 0 : o.type) === "enumeration" && Array.isArray(o.parameter) && (o.parameter = o.parameter.map(String)), r.value = o, r.uri = e.uri, r.instructions = e.instructions, t.push(r);
  }
  return t;
}, fh = (c, i) => {
  const t = [];
  for (const e of i) {
    const s = e.propertySet, n = e.baseName, r = ge(s), o = ge(n);
    if (!(o && r))
      continue;
    const a = new ch(c, r, o);
    e.cardinality && (a.cardinality = e.cardinality);
    const l = ge(e.value);
    (l == null ? void 0 : l.type) === "enumeration" && Array.isArray(l.parameter) && (l.parameter = l.parameter.map(String)), a.value = l, a.dataType = e.dataType, a.uri = e.uri, a.instructions = e.instructions, t.push(a);
  }
  return t;
}, ri = class ri extends At {
  constructor(t) {
    super(t);
    C(this, "enabled", !0);
    C(this, "list", new re());
    t.add(ri.uuid, this);
  }
  /**
   * Retrieves a FragmentIdMap based on the given IDSCheckResult array.
   * The map separates the IDs into two categories: pass and fail.
   *
   * @param model - The FragmentsGroup model from which to retrieve the fragment map.
   * @param result - An array of IDSCheckResult objects, each representing a check result.
   *
   * @returns An object containing two properties:
   * - `pass`: A FragmentIdMap that passed the checks.
   * - `fail`: A FragmentIdMap that failed the checks.
   */
  getFragmentIdMap(t, e) {
    const n = e.filter((h) => h.pass).map((h) => h.expressID), r = t.getFragmentMap(n), a = e.filter((h) => !h.pass).map((h) => h.expressID), l = t.getFragmentMap(a);
    return { pass: r, fail: l };
  }
  /**
   * Creates a new IDSSpecification instance and adds it to the list.
   *
   * @param name - The name of the IDSSpecification.
   * @param ifcVersion - An array of IfcVersion values that the specification supports.
   *
   * @returns The newly created IDSSpecification instance.
   */
  create(t, e, s) {
    const n = new lh(
      this.components,
      t,
      e
    );
    return s && (n.identifier = s), this.list.set(n.identifier, n), n;
  }
  /**
   * Parses and processes an XML string containing Information Delivery Specification (IDS) data.
   * It creates IDSSpecification instances based on the parsed data and returns them in an array.
   * Also, the instances are added to the list array.
   *
   * @param data - The XML string to parse.
   *
   * @returns An array of IDSSpecification instances created from the parsed data.
   */
  load(t) {
    const e = [], s = ri.xmlParser.parse(t).ids, { specifications: n } = s;
    if (n && n.specification) {
      const r = Array.isArray(n.specification) ? n.specification : [n.specification];
      for (const o of r) {
        const { name: a, ifcVersion: l, description: h, instructions: f, identifier: I } = o;
        if (!(a && l))
          continue;
        const u = [], d = [], { applicability: E, requirements: T } = o;
        if (E) {
          const { maxOccurs: R, ...S } = E, m = Array.isArray(S) ? S : [S];
          for (const F of m)
            for (const O in F) {
              const y = Array.isArray(F[O]) ? F[O] : [F[O]];
              if (O === "entity") {
                const w = Zr(this.components, y);
                u.push(...w);
              }
            }
        }
        let p;
        if (T) {
          const { maxOccurs: R, ...S } = T;
          p = T.description;
          const m = Array.isArray(S) ? S : [S];
          for (const F of m)
            for (const O in F) {
              const y = Array.isArray(F[O]) ? F[O] : [F[O]];
              if (O === "entity") {
                const w = Zr(this.components, y);
                d.push(...w);
              }
              if (O === "attribute") {
                const w = hh(this.components, y);
                d.push(...w);
              }
              if (O === "classification") {
                const w = uh(
                  this.components,
                  y
                );
                d.push(...w);
              }
              if (O === "property") {
                const w = fh(this.components, y);
                d.push(...w);
              }
            }
        }
        if (u.length > 0 && d.length > 0) {
          const R = this.create(
            a,
            l.split(/\s+/),
            I
          );
          R.description = h, R.instructions = f, R.requirementsDescription = p, R.applicability.add(...u), R.requirements.add(...d), e.push(R);
        }
      }
    }
    return e;
  }
  /**
   * Exports the IDSSpecifications data into an XML string.
   *
   * @param info - The metadata information for the exported XML.
   * @param specifications - An optional iterable of IDSSpecification instances to export.
   * If not provided, all specifications in the list will be exported.
   *
   * @returns A string containing the exported IDSSpecifications data in XML format.
   */
  export(t, e = this.list.values()) {
    const s = e ?? this.list;
    return `<ids:ids xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/1.0/ids.xsd" xmlns:ids="http://standards.buildingsmart.org/IDS">
  <!-- Made with That Open Engine ${Cs.release} (https://github.com/thatopen/engine_components) -->
  <ids:info>
    <ids:title>${t.title}</ids:title>
    ${t.copyright ? `<ids:copyright>${t.copyright}</ids:copyright>` : ""}
    ${t.version ? `<ids:version>${t.version}</ids:version>` : ""}
    ${t.description ? `<ids:description>${t.description}</ids:description>` : ""}
    ${t.author ? `<ids:author>${t.author}</ids:author>` : ""}
    ${t.date ? `<ids:date>${t.date.toISOString().split("T")[0]}</ids:date>` : ""}
    ${t.purpose ? `<ids:purpose>${t.purpose}</ids:purpose>` : ""}
    ${t.milestone ? `<ids:milestone>${t.milestone}</ids:milestone>` : ""}
  </ids:info>
  <ids:specifications>
    ${[...s].map((r) => r.serialize()).join(`
`)}
  </ids:specifications>
</ids:ids>`;
  }
};
C(ri, "uuid", "9f0b9f78-9b2e-481a-b766-2fbfd01f342c"), C(ri, "xmlParser", new To.XMLParser({
  allowBooleanAttributes: !0,
  attributeNamePrefix: "",
  ignoreAttributes: !1,
  ignoreDeclaration: !0,
  ignorePiTags: !0,
  numberParseOptions: { leadingZeros: !0, hex: !0 },
  parseAttributeValue: !0,
  preserveOrder: !1,
  processEntities: !1,
  removeNSPrefix: !0,
  trimValues: !0
}));
let bn = ri;
export {
  _e as AsyncEvent,
  Mt as BCFTopics,
  Kl as BCFTopicsConfigManager,
  Vn as Base,
  Va as BaseCamera,
  Ya as BaseRenderer,
  Ga as BaseScene,
  Yn as BaseWorldItem,
  Mn as BoundingBoxer,
  Bi as Classifier,
  On as Clipper,
  wn as Comment,
  At as Component,
  ph as ComponentWithUI,
  Cs as Components,
  Ye as ConfigManager,
  Ge as Configurator,
  lc as CullerRenderer,
  Sn as Cullers,
  re as DataMap,
  we as DataSet,
  De as Disposer,
  j as Event,
  Hr as Exploder,
  Oc as FirstPersonMode,
  Tt as FragmentsManager,
  lo as GeometryTypes,
  yr as Grids,
  Dn as Hider,
  rh as IDSAttribute,
  oh as IDSClassification,
  ah as IDSEntity,
  ks as IDSFacet,
  ch as IDSProperty,
  lh as IDSSpecification,
  bn as IDSSpecifications,
  gs as IfcBasicQuery,
  gh as IfcCategories,
  ms as IfcCategoryMap,
  _c as IfcElements,
  kr as IfcFinder,
  se as IfcFinderQuery,
  zn as IfcFragmentSettings,
  Wr as IfcGeometryTiler,
  Nn as IfcJsonExporter,
  yn as IfcLoader,
  Ln as IfcPropertiesManager,
  eh as IfcPropertiesTiler,
  oi as IfcPropertiesUtils,
  As as IfcPropertyQuery,
  Gt as IfcRelationsIndexer,
  Jl as IfcStreamingSettings,
  Tr as MaterialsUtils,
  $r as MeasurementUtils,
  hc as MeshCullerRenderer,
  nh as MiniMap,
  sh as MiniMapConfigManager,
  Xr as MiniMaps,
  fc as Mouse,
  Nc as OrbitMode,
  Pc as OrthoPerspectiveCamera,
  yc as PlanMode,
  Lc as ProjectionManager,
  th as PropertiesStreamingSettings,
  ci as Raycasters,
  Rh as ShadowedScene,
  xi as SimpleCamera,
  Ec as SimpleGrid,
  dc as SimpleGridConfigManager,
  Gn as SimplePlane,
  Ic as SimpleRaycaster,
  mh as SimpleRenderer,
  ec as SimpleScene,
  tc as SimpleSceneConfigManager,
  Qa as SimpleWorld,
  Rs as Topic,
  oe as UUID,
  Th as VertexPicker,
  mo as Viewpoint,
  ie as Viewpoints,
  Ts as Worlds,
  Ql as extensionsImporter,
  wc as ifcCategoryCase,
  Dc as ifcRelAttrsPosition,
  bc as ifcRelClassNames,
  Ch as isPointInFrontOfPlane,
  qa as obbFromPoints,
  no as readPixelsAsync,
  vr as relToAttributesMap
};
