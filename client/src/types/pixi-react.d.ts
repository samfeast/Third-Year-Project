import type { Graphics } from 'pixi.js'

declare module 'react' {
  namespace JSX {
    interface IntrinsicElements {
      graphics: {
        draw?: (g: Graphics) => void
      }
      container: {
        scale?: number
        x?: number
        y?: number
        children?: React.ReactNode
      }
    }
  }
}