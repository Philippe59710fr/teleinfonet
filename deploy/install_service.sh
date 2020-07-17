mkdir -p /opt/teleinfonet
cp ../build/armv7/teleinfonet /opt/teleinfonet
cp run.sh /opt/teleinfonet
cp gethelp.sh /opt/teleinfonet

cp teleinfo.service /etc/systemd/system/
systemctl enable teleinfo
systemctl start teleinfo
