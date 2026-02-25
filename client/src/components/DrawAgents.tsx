import { extend } from '@pixi/react'
import { Graphics } from 'pixi.js'

extend({ Graphics })

export type Agent = {
  id: number
  x: number
  y: number
  colour: number
}

type DrawAgentsProps = {
  agents: Agent[]
  radius: number
}

export default function DrawAgents({ agents, radius }: DrawAgentsProps) {
    return (
    <pixiGraphics
      draw={(g) => {
        if (agents.length === 0) return
        g.clear()
        agents.forEach(agent => {
            g.circle(agent.x, agent.y, radius)
            g.fill({ color: agent.colour })
        })
      }}
    />
  )
}