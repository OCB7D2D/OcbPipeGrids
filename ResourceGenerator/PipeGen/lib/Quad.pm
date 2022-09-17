use strict;
use warnings;
package Quad;
use base qw(Face);
use Math::Vector::Real;

sub Lines
{
	return @{$_[0]->{lines}} 
		if exists ($_[0]->{lines});
	return @{$_[0]->{lines} = [
		#Line->new($_[0]->Vertice(0), $_[0]->Vertice(1)),
		Line->new($_[0]->Vertice(1), $_[0]->Vertice(2)),
		#Line->new($_[0]->Vertice(2), $_[0]->Vertice(0)),
		Line->new($_[0]->Vertice(0), $_[0]->Vertice(3)),
	]};
}

sub VectorIsClose($$$)
{
	return abs($_[0]->[0] - $_[1]->[0]) < $_[2]
		&& abs($_[0]->[1] - $_[1]->[1]) < $_[2]
		&& abs($_[0]->[2] - $_[1]->[2]) < $_[2];
}

sub CalcFaceNormal
{
	my $self = shift;
	my $U = $self->Vertice(1)->Position - $self->Vertice(0)->Position;
	my $V = $self->Vertice(2)->Position - $self->Vertice(0)->Position;
	my $N1 = V(
		$U->[1] * $V->[2] - $U->[2] * $V->[1],
		$U->[2] * $V->[0] - $U->[0] * $V->[2],
		$U->[0] * $V->[1] - $U->[1] * $V->[0],
	);
	if (abs($N1) == 0)
	{
		warn $self->Vertice(0)->Position,
		" vs ", $self->Vertice(1)->Position,
		" vs ", $self->Vertice(2)->Position,
		" vs ", $self->Vertice(3)->Position,
		;
		warn "= $U $V => $N1";
	}
	$U = $self->Vertice(2)->Position - $self->Vertice(1)->Position;
	$V = $self->Vertice(3)->Position - $self->Vertice(1)->Position;
	my $N2 = V(
		$U->[1] * $V->[2] - $U->[2] * $V->[1],
		$U->[2] * $V->[0] - $U->[0] * $V->[2],
		$U->[0] * $V->[1] - $U->[1] * $V->[0],
	);
	
	if (0 && !VectorIsClose($N1, $N2, 1e-6)) {
		warn "Quard surface vectors don't match\n";
		warn "V0: ", $self->Vertice(0)->Position, "\n";
		warn "V1: ", $self->Vertice(1)->Position, "\n";
		warn "V2: ", $self->Vertice(2)->Position, "\n";
		warn "V3: ", $self->Vertice(3)->Position, "\n";
		warn "N1: $N1\nN2: $N2\n";

	}
	my $N = ($N1 + $N2) / 2;
	#return $N if (abs($N) == 0);
	# $self->Vertice(0)->Normal->set($N);
	# $self->Vertice(1)->Normal->set($N);
	# $self->Vertice(2)->Normal->set($N);
	# $self->Vertice(3)->Normal->set($N);
	return $N->versor;
}

sub new
{
	my $package = shift;
	my $self = $package->SUPER::new(@_);
	# Other subconstructor stuff here
	return $self;
}

sub Compress
{
	if (abs($_[0]->Vertice(0)->Position - $_[0]->Vertice(1)->Position) < 1e-12)
	{
		splice @{$_[0]->{vertices}}, 1, 1;
		bless $_[0], "Triangle";
		die "asd";
	}
	elsif (abs($_[0]->Vertice(1)->Position - $_[0]->Vertice(2)->Position) < 1e-12)
	{
		splice @{$_[0]->{vertices}}, 2, 1;
		bless $_[0], "Triangle";
	}
	elsif (abs($_[0]->Vertice(2)->Position - $_[0]->Vertice(3)->Position) < 1e-12)
	{
		splice @{$_[0]->{vertices}}, 3, 1;
		bless $_[0], "Triangle";
	}
	elsif (abs($_[0]->Vertice(3)->Position - $_[0]->Vertice(0)->Position) < 1e-12)
	{
		splice @{$_[0]->{vertices}}, 0, 1;
		bless $_[0], "Triangle";
	}
	# || abs($self->Vertice(1)->Position - $self->Vertice(2)->Position) < 1e-12
	# || abs($self->Vertice(2)->Position - $self->Vertice(2)->Position) < 1e-12
	# || abs($self->Vertice(3)->Position - $self->Vertice(0)->Position) < 1e-12)
}

sub SetUV
{
	my ($self, $u1, $v1, $u2, $v2) = @_;
	$self->{uvs} = [V($u1, $v1), V($u1, $v2),
		V($u2, $v2), V($u2, $v1)];
	return $self;
}

sub SetUV2
{
	my ($self, $u1, $v1, $u2, $v2) = @_;
	$self->{uvs} = [V($u2, $v1), V($u1, $v1), V($u1, $v2),
		V($u2, $v2)];
	return $self;
}

sub VertexCount { 4 }


#sub ToWaveFrontObj
#{
#	my ($self) = @_;
#	if ($self->Position)
#	{
#		return sprintf("%d/%d/%d",
#			$self->Position->[3]+1,
#			$self->{uv}->[2]+1,
#			$self->Normal->[3]+1);
#	}
#	else {
#		return "NA";
#	}
#}


1;