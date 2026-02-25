import { Application, extend } from '@pixi/react'
import { Graphics, Container } from 'pixi.js'
import DrawAgents, { type Agent } from './DrawAgents'

extend({ Graphics, Container })

type Props = {
    floorplan: number[][]
    agents: Agent[]
}

function Floorplan({ floorplan }: { floorplan: number[][]}) {
    return (
        <pixiGraphics
        draw={(g) => {
            g.clear()

            floorplan.forEach(([x1, y1, x2, y2]) => {
            g.moveTo(x1, y1)
            g.lineTo(x2, y2)
            })

            g.stroke({
            width: 11,
            color: 0x000000,
            })
        }}
        />
    )
}

export default function SimulationCanvas({ floorplan, agents } : Props) {
    const canvasWidth = 1400
    const canvasHeight = 700

    const xValues = floorplan.flatMap(([x1, , x2]) => [x1, x2])
    const yValues = floorplan.flatMap(([ , y1, , y2]) => [y1, y2])

    const minX = Math.min(...xValues)
    const maxX = Math.max(...xValues)
    const minY = Math.min(...yValues)
    const maxY = Math.max(...yValues)

    const worldWidth = maxX - minX
    const worldHeight = maxY - minY
    
    const scaleX = 0.9 * canvasWidth / worldWidth
    const scaleY = 0.9 * canvasHeight / worldHeight
    const scale = Math.min(scaleX, scaleY)

    const offsetX = -minX * scale + (canvasWidth - (scale * worldWidth)) / 2
    const offsetY = -minY * scale + (canvasHeight - (scale * worldHeight)) / 2
  
    return (
    <Application width={canvasWidth} height={canvasHeight} background={0xaaaaaa}>
      <container scale={scale} x={offsetX} y={offsetY}>
      <Floorplan floorplan={floorplan} />
      <DrawAgents agents={agents} radius={22.5} />
      </container>
    </Application>
  )
}