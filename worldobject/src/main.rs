extern crate rosc;

use rosc::encoder;
use rosc::{OscBundle, OscMessage, OscPacket, OscType};
use std::net::{SocketAddrV4, UdpSocket};
use std::str::FromStr;
use std::time::Duration;
use std::{env, f32, thread};

#[derive(Copy, Clone)]
struct Vector {
    x: f32,
    y: f32,
    z: f32,
    r: f32,
}

struct Data {
    player: Vector,
    object: Vector,
}

fn get_addr_from_arg(arg: &str) -> SocketAddrV4 {
    SocketAddrV4::from_str(arg).unwrap()
}

fn main() {
    let mut args = env::args();
    let host_addr = get_addr_from_arg(&args.nth(1).unwrap_or("127.0.0.1:9001".to_string()));
    let to_addr = get_addr_from_arg(&args.nth(2).unwrap_or("127.0.0.1:9000".to_string()));
    let sock = UdpSocket::bind(host_addr).unwrap();

    let mut buf = [0u8; rosc::decoder::MTU];
    let mut data = Data {
        player: Vector {
            x: 0.0,
            y: 0.0,
            z: 0.0,
            r: 0.0,
        },
        object: Vector {
            x: 0.0,
            y: 0.0,
            z: 0.0,
            r: 0.0,
        },
    };

    loop {
        // Receive and handle any incoming packets.
        match sock.recv_from(&mut buf) {
            Ok((size, addr)) => {
                println!("Received packet with size {} from: {}", size, addr);

                match rosc::decoder::decode(&buf[..size]) {
                    Ok(packet) => {
                        handle_packet(&packet, &mut data);
                    }
                    Err(e) => {
                        println!("Error decoding OSC packet: {}", e);
                    }
                }
            }
            Err(e) => {
                println!("Error receiving from socket: {}", e);
                break;
            }
        }

        let msg_buf = encoder::encode(&OscPacket::Bundle(OscBundle {
            timetag: rosc::OscTime::try_from(std::time::UNIX_EPOCH).unwrap(),
            content: vec![
                message(
                    "/avatar/parameters/WorldPosCoarse",
                    OscType::Float(data.object.x),
                ),
                OscPacket::Message(OscMessage {
                    addr: "/avatar//3/xy1".to_string(),
                    args: vec![OscType::Float(0.0), OscType::Float(0.0)],
                }),
                OscPacket::Message(OscMessage {
                    addr: "/avatar//3/xy1".to_string(),
                    args: vec![OscType::Float(0.0), OscType::Float(0.0)],
                }),
            ],
        }))
        .unwrap();

        // Send an updated axis
        let msg_buf = sock.send_to(&msg_buf, to_addr).unwrap();

        println!("doink");

        // Time updates to coincide with VRC network ticks
        thread::sleep(Duration::from_millis((1000.0 / 4.0) as u64));
    }
}

fn message(addr: &str, value: OscType) -> OscPacket {
    OscPacket::Message(OscMessage {
        addr: "/avatar//3/xy1".to_string(),
        args: vec![value],
    })
}

fn handle_packet(packet: &OscPacket, data: &mut Data) {
    match packet {
        OscPacket::Message(msg) => {
            handle_message(msg, data);
        }
        OscPacket::Bundle(bundle) => {
            for msg in bundle.content.iter() {
                handle_packet(msg, data);
            }
        }
    }
}

fn handle_message(msg: &OscMessage, data: &mut Data) {
    match (msg.addr.as_str(), msg.args.as_slice()) {
        ("/WorldObject/PlayerX", [rosc::OscType::Float(x)]) => data.player.x = *x,
        ("/WorldObject/PlayerY", [rosc::OscType::Float(y)]) => data.player.y = *y,
        ("/WorldObject/PlayerZ", [rosc::OscType::Float(z)]) => data.player.z = *z,
        ("/WorldObject/Drop", _) => data.object = data.player,
        _ => {
            println!("Unhandled OSC message: {} => {:?}", msg.addr, msg.args);
        }
    }
}
